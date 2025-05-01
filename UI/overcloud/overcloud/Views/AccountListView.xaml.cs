using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using DB.overcloud.Repository;
using OverCloud.Services;
using OverCloud.Services.FileManager.DriveManager;
using OverCloud.Services.FileManager;
using OverCloud.Services.StorageManager;
using static overcloud.Views.HomeView;
using DB.overcloud.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Forms;

namespace overcloud.Views
{
    public partial class AccountListView : System.Windows.Controls.UserControl
    {

        // Services and managers
        private AccountService _accountService;
        private QuotaManager _quotaManager;

        private ICollectionView _view;
        private ObservableCollection<AccountItemViewModel> _items;

        public AccountListView()
        {
            InitializeComponent();
            Loaded += AccountListView_Loaded;

            // 초기 서비스 설정
            var connStr = DbConfig.ConnectionString;
            var storageRepo = new StorageRepository(connStr);
            var accountRepo = new AccountRepository(connStr);

            var tokenFactory = new TokenProviderFactory();
            var googleSvc = new GoogleDriveService(tokenFactory.CreateGoogleTokenProvider(), storageRepo, accountRepo);
            var oneDriveSvc = new OneDriveService(tokenFactory.CreateOneDriveTokenRefresher(), storageRepo, accountRepo);
            var cloudSvcs = new List<ICloudFileService> { googleSvc, oneDriveSvc };

            _quotaManager = new QuotaManager(cloudSvcs, storageRepo, accountRepo);
            _accountService = new AccountService(accountRepo, storageRepo, _quotaManager);
            var tierMgr = new CloudTierManager(accountRepo);

            FilterTab.SelectedIndex = 0;
        }

        private void AccountListView_Loaded(object sender, RoutedEventArgs e)
        {
            // (1) 사용자 ID 구하기
            string currentUserId = /* AuthenticationService.CurrentUserId */ "1";

            var all = _accountService.Get_Clouds_For_User(currentUserId);
            var _items = all.Select(a => new AccountItemViewModel
            {
                CloudName = a.CloudType,
                IsActive = true,
                AccountId = a.AccountId,
                UsagePercent = (int)(a.UsedCapacity * 100 / a.TotalCapacity),
                UsageDisplay = $"{a.UsedCapacity}/{a.TotalCapacity } GB",
                LastLoginDate = DateTime.Now,
                IsSelected = false
            }).ToList();

            _view = CollectionViewSource.GetDefaultView(_items);
            AccountsGrid.ItemsSource = _view;

            // 최초 필터 적용 (All)
            ApplyFilter("All");
        }

        public class AccountItemViewModel : INotifyPropertyChanged
        {
            private bool _isSelected;
            public bool IsSelected
            {
                get => _isSelected;
                set { _isSelected = value; OnPropertyChanged(); }
            }

            public string CloudName { get; set; }
            public bool IsActive { get; set; }  // "Active"
            public string AccountId { get; set; }
            public int UsagePercent { get; set; }
            public string UsageDisplay { get; set; }
            public DateTime LastLoginDate { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string propName = null)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }



        private void Button_Add_Click(object sender, RoutedEventArgs e)
        {
            AddAccountWindow window = new AddAccountWindow(_accountService);
            window.ShowDialog();
        }

        private void Button_Delete_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("삭제 버튼 누름");
            var window = new DeleteAccountWindow(_accountService);
            // this(UserControl)가 아니라 이 컨트롤을 호스트하는 Window를 Owner로 지정
            window.Owner = Window.GetWindow(this);
            window.ShowDialog();
            // 필요하다면 HomeView 쪽 RefreshExplorer() 호출
        }

        private void FilterTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_view == null) return;
            var header = (FilterTab.SelectedItem as TabItem)?.Header as string;
            ApplyFilter(header);
        }

        private void ApplyFilter(string header)
        {
            switch (header)
            {
                case "Active":
                    _view.Filter = o => ((AccountItemViewModel)o).IsActive;
                    break;
                case "Disactive":
                    _view.Filter = o => !((AccountItemViewModel)o).IsActive;
                    break;
                default: // "All"
                    _view.Filter = null;
                    break;
            }
        }
    }
}

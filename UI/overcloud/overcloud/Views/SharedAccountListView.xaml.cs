using DB.overcloud.Repository;
using OverCloud.Services.FileManager;
using OverCloud.Services.StorageManager;
using OverCloud.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows;

namespace overcloud.Views
{
    public partial class SharedAccountListView : System.Windows.Controls.UserControl
    {
        private LoginController _controller;
        private string _user_id;

        private ICollectionView _view;
        private ObservableCollection<AccountItemViewModel> _items;

        public SharedAccountListView(LoginController controller, string user_id)
        {
            InitializeComponent();

            _controller = controller;
            _user_id = user_id;

            FilterTab.SelectedIndex = 0;
        }

        private void SharedAccountListView_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshList();
        }

        private void RefreshList()
        {
            string currentUserId = _user_id;

            // 공유 계정 리스트 불러오기
            var all = _controller.AccountService.Get_Clouds_For_User(currentUserId);

            _items = new ObservableCollection<AccountItemViewModel>(
                all.Select(a => new AccountItemViewModel
                {
                    CloudName = a.CloudType,
                    IsActive = true,
                    AccountId = a.AccountId,
                    UsagePercent = a.TotalCapacity > 0
                                      ? (int)(a.UsedCapacity * 100.0 / a.TotalCapacity)
                                      : 0,
                    UsageDisplay = $"{(a.UsedCapacity / 1024 / 1024):F2}/{(a.TotalCapacity / 1024 / 1024):F2} GB",
                    LastLoginDate = DateTime.Now,
                    IsSelected = false
                }));

            _view = CollectionViewSource.GetDefaultView(_items);
            AccountsGrid.ItemsSource = _view;

            var header = (FilterTab.SelectedItem as TabItem)?.Header as string;
            ApplyFilter(header);
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
            public bool IsActive { get; set; }
            public string AccountId { get; set; }
            public int UsagePercent { get; set; }
            public string UsageDisplay { get; set; }
            public DateTime LastLoginDate { get; set; }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string prop = null)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        private void Button_Add_Click(object sender, RoutedEventArgs e)
        {
            var window = new AddAccountWindow(_controller, _user_id, true);
            window.Owner = Window.GetWindow(this);
            window.ShowDialog();

            RefreshList();
        }

        private void Button_Delete_Click(object sender, RoutedEventArgs e)
        {
            var window = new DeleteAccountWindow(_controller, _user_id);
            window.Owner = Window.GetWindow(this);
            window.ShowDialog();

            RefreshList();
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
                default:
                    _view.Filter = null;
                    break;
            }
            _view.Refresh();
        }
    }
}

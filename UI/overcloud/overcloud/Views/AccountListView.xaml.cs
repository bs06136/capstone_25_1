using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using DB.overcloud.Models;
using DB.overcloud.Repository;
using OverCloud.Services;
using OverCloud.Services.FileManager;
using OverCloud.Services.FileManager.DriveManager;
using OverCloud.Services.StorageManager;

namespace overcloud.Views
{
    public partial class AccountListView : System.Windows.Controls.UserControl
    {
        // Services and managers
        private LoginController _controller;
        private string _user_id;
        private ICollectionView _view;
        private ObservableCollection<AccountItemViewModel> _items;

        public AccountListView(LoginController controller, string user_id)
        {
            InitializeComponent();
            Loaded += AccountListView_Loaded;

            // 초기 서비스 설정
            _controller = controller;
            _user_id = user_id;


            FilterTab.SelectedIndex = 0;
        }

        private void AccountListView_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshList();
        }

        private void RefreshList()
        {
            // (1) 사용자 ID 구하기
            string currentUserId = /* AuthenticationService.CurrentUserId */ _user_id;

            // (2) DB에서 계정 목록 조회
            var all = _controller.AccountService.Get_Clouds_For_User(currentUserId);

            // (3) 뷰모델 변환
            _items = new ObservableCollection<AccountItemViewModel>(
                all.Select(a => new AccountItemViewModel
                {
                    CloudName = a.CloudType,
                    IsActive = true, // 모두 Active로 표시
                    AccountId = a.AccountId,
                    UsagePercent = a.TotalCapacity > 0
                                      ? (int)(a.UsedCapacity * 100.0 / a.TotalCapacity)
                                      : 0,
                    UsageDisplay = $"{((double)a.UsedCapacity / 1024 /1024):F2}/{((double)a.TotalCapacity / 1024 /1024):F2} GB",
                    LastLoginDate = DateTime.Now,
                    IsSelected = false
                }));

            // (4) CollectionViewSource 준비
            _view = CollectionViewSource.GetDefaultView(_items);
            AccountsGrid.ItemsSource = _view;

            // (5) 현재 탭 필터 다시 적용
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
            var window = new AddAccountWindow(_controller, _user_id, false);
            window.Owner = Window.GetWindow(this);
            window.ShowDialog();

            RefreshList();
        }

        private void Button_Delete_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("삭제 버튼 누름");
            var window = new DeleteAccountWindow(_controller, _user_id, false);
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
                default: // "All"
                    _view.Filter = null;
                    break;
            }
            _view.Refresh();
        }
    }
}

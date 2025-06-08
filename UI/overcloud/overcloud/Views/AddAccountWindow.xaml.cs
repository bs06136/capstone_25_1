using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using DB.overcloud.Repository;
using DB.overcloud.Models;  // CloudAccountInfo 클래스 사용을 위해 추가
using OverCloud.Services;
using SourceChord.FluentWPF; // AcrylicWindow 상속을 위해 추가

namespace overcloud.Views
{
    public partial class AddAccountWindow : AcrylicWindow
    {
        private readonly LoginController _controller;
        private readonly string _userId;
        private readonly bool _isCoopMode;

        public AddAccountWindow(LoginController controller, string userId, bool coop)
        {
            InitializeComponent();
            _controller = controller;
            _userId = userId;
            _isCoopMode = coop;

            // 창 드래그 가능
            this.MouseDown += (s, e) => { if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed) this.DragMove(); };
        }

        private void AddAccountWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isCoopMode)
            {
                cooperationComboBox.Visibility = Visibility.Visible;
                var coopAccounts = _controller.CoopUserRepository.connected_cooperation_account_nums(_userId);
                cooperationComboBox.ItemsSource = coopAccounts;
            }
            else
            {
                cooperationComboBox.Visibility = Visibility.Collapsed;
            }
        }

        private async void Confirm_Click(object sender, RoutedEventArgs e)
        {
            string id = txtID.Text;
            string password = txtPassword.Password;
            string cloudType = (cloudComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? string.Empty;

            string targetId = _isCoopMode && cooperationComboBox.SelectedItem != null
                ? cooperationComboBox.SelectedItem.ToString()
                : _userId;

            var accountInfo = new CloudStorageInfo
            {
                ID = targetId,
                AccountId = id,
                AccountPassword = password,
                CloudType = cloudType,
                TotalCapacity = 0,
                UsedCapacity = 0
            };

            bool success = await _controller.AccountService.Add_Cloud_Storage(accountInfo, _userId);
            System.Windows.MessageBox.Show(success ? "계정 추가 성공" : "계정 추가 실패");
            this.Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

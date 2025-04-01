using System.Windows;
using System.Collections.Generic;
using overcloud.Models;
using OverCloud.Services;
using DB.overcloud.Service;
using System.Diagnostics;

namespace overcloud.Views
{
    public partial class DeleteAccountWindow : Window
    {
        private readonly AccountService _accountService;

        public DeleteAccountWindow(AccountService service)
        {
            InitializeComponent();
            _accountService = service;
            LoadAccounts();
        }

        private void LoadAccounts()
        {
            List<CloudAccountInfo> accounts = _accountService.GetAllAccounts();
            AccountListBox.ItemsSource = accounts;
            Debug.WriteLine("list 출력");
        }

        private void ConfirmDelete_Click(object sender, RoutedEventArgs e)
        {
            var selectedAccount = AccountListBox.SelectedItem as CloudAccountInfo;
            if (selectedAccount == null)
            {
                MessageBox.Show("삭제할 계정을 선택해 주세요.");
                return;
            }

            int userNum = selectedAccount.UserNum;
            bool result = _accountService.RemoveAccount(userNum);

            MessageBox.Show(result ? "계정 삭제 성공" : "계정 삭제 실패");
            this.Close();
        }
    }
}

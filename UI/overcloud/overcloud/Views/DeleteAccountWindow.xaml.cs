using System.Collections.Generic;
using System.Windows;
using DB.overcloud.Models;
using DB.overcloud.Service;

namespace overcloud.Views
{
    public partial class DeleteAccountWindow : Window
    {
        private readonly IAccountRepository _accountRepo;

        public DeleteAccountWindow(IAccountRepository accountRepo)
        {
            InitializeComponent();
            _accountRepo = accountRepo;

            LoadAccounts();
        }

        private void LoadAccounts()
        {
            List<CloudAccountInfo> accounts = _accountRepo.GetAllAccounts();
            AccountListBox.ItemsSource = accounts;
        }

        private void ConfirmDelete_Click(object sender, RoutedEventArgs e)
        {
            if (AccountListBox.SelectedItem is not CloudAccountInfo selectedAccount)
            {
                MessageBox.Show("삭제할 계정을 선택해 주세요.", "선택 필요", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int userNum = selectedAccount.UserNum;
            bool result = _accountRepo.DeleteAccountByUserNum(userNum);

            MessageBox.Show(result ? "계정 삭제 성공" : "계정 삭제 실패");

            if (result)
            {
                LoadAccounts(); // 삭제 후 목록 새로고침
                this.Close();   // 또는 유지할 수도 있음
            }
        }
    }
}

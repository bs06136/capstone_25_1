using System.Windows;
using System.Collections.Generic;
using DB.overcloud.Models;
//using static overcloud.temp_class.TempClass;
using OverCloud.Services;
using DB.overcloud.Repository;
using System.Diagnostics;

namespace overcloud.Views
{
    public partial class DeleteAccountWindow : Window
    {
        private AccountService _accountService;     //수정 필요

        public DeleteAccountWindow(AccountService accountService)
        {
            InitializeComponent();



            //string connStr = "server=localhost;database=overcloud;uid=admin;pwd=admin;"; ;  //
            //IAccountRepository repo = new AccountRepository(connStr);                       // 수정필요
            _accountService = accountService;                                    //

            LoadAccounts();
        }

        private void LoadAccounts()
        {
            var main = System.Windows.Application.Current.MainWindow as MainWindow;
            if (main != null)
            {
                // 기존: List<CloudStorageInfo> accounts = main.GetAllCloudStatus();
                // 변경: List<CloudAccountInfo> accounts = main.GetAllAccounts();
                List<CloudStorageInfo> accounts = _accountService.GetCloudsForUser();
                AccountListBox.ItemsSource = accounts;
                Debug.WriteLine("list 출력");
            }
        }

        private void ConfirmDelete_Click(object sender, RoutedEventArgs e)
        {
            var selectedAccount = AccountListBox.SelectedItem as CloudAccountInfo;
            if (selectedAccount == null)
            {
                System.Windows.MessageBox.Show("삭제할 계정을 선택해 주세요.");
                return;
            }

            // userNum을 이용해 삭제 처리
            int userNum = selectedAccount.UserNum;

            // temp_class의 RemoveAccount 호출
            bool result = _accountService.DeleteCloudStorage(userNum);

            System.Windows.MessageBox.Show(result ? "계정 삭제 성공" : "계정 삭제 실패");
            this.Close();
        }
    }
}

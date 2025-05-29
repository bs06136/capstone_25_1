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
        private LoginController _controller;
        private string _user_id;    
        private bool _is_shared; 

        public DeleteAccountWindow(LoginController controller, string user_id, bool is_shared)
        {
            InitializeComponent();
            _controller = controller;
            _user_id = user_id;
            _is_shared = is_shared;

            LoadAccounts();
        }

        private void LoadAccounts()
        {
            System.Diagnostics.Debug.WriteLine("계정 불러오기 시작");

            List<CloudStorageInfo> allAccounts = new();

            if (_is_shared)
            {
                // 협업 클라우드에 속한 모든 계정 불러오기
                List<string> coopIds = _controller.CoopUserRepository.connected_cooperation_account_nums(_user_id);
                foreach (var coopId in coopIds)
                {
                    var accounts = _controller.AccountService.Get_Clouds_For_User(coopId);
                    allAccounts.AddRange(accounts);
                }
            }
            else
            {
                // 일반 개인 계정만 불러오기
                allAccounts = _controller.AccountService.Get_Clouds_For_User(_user_id);
            }

            AccountListBox.ItemsSource = allAccounts;
            Debug.WriteLine("계정 목록 출력 완료");
        }



        private async void ConfirmDelete_Click(object sender, RoutedEventArgs e)
        {
            var selectedAccount = AccountListBox.SelectedItem as CloudStorageInfo;
            if (selectedAccount == null)
            {
                System.Windows.MessageBox.Show("삭제할 계정을 선택해 주세요.");
                return;
            }

            // userNum을 이용해 삭제 처리
            int CloudStorageNum = selectedAccount.CloudStorageNum;

            // temp_class의 RemoveAccount 호출
            bool result = await _controller.AccountService.Delete_Cloud_Storage(CloudStorageNum, _user_id);

            System.Windows.MessageBox.Show(result ? "계정 삭제 성공" : "계정 삭제 실패");
            this.Close();
        }
    }
}

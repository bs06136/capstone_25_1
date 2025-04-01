using System.Windows;
using DB.overcloud.Models;
using DB.overcloud.Service;

namespace overcloud.Views
{
    public partial class AddAccountWindow : Window
    {
        private readonly IAccountRepository _accountRepo;

        public AddAccountWindow(IAccountRepository accountRepo)
        {
            InitializeComponent();
            _accountRepo = accountRepo;
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text.Trim();         // 사용자 이름
            string id = txtID.Text.Trim();                     // 클라우드 계정 ID
            string password = txtPassword.Password.Trim();     // 로그인 비밀번호

            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(id) ||
                string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("모든 입력 값을 채워주세요.", "입력 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var account = new CloudAccountInfo
            {
                Username = username,
                ID = id,
                Password = password,
                TotalSize = 0,
                UsedSize = 0
            };

            bool result = _accountRepo.InsertAccount(account);
            MessageBox.Show(result ? "계정 추가 성공" : "계정 추가 실패");

            if (result)
                this.Close();
        }
    }
}

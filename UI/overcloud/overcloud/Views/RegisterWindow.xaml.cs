using System.Windows;
using DB.overcloud.Repository;

namespace overcloud.Views
{
    public partial class RegisterWindow : Window
    {
        private AccountRepository _accountRepository;
        public RegisterWindow(AccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e )
        {
            string userId = IdBox.Text.Trim();
            string password = PasswordBox.Password.Trim();
            string confirm = ConfirmBox.Password.Trim();

            if (password != confirm)
            {
                System.Windows.MessageBox.Show("비밀번호가 일치하지 않습니다. 다시 입력해주세요.", "입력 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(password))
            {
                System.Windows.MessageBox.Show("아이디와 비밀번호를 모두 입력해주세요.", "입력 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool success = _accountRepository.assign_overcloud(userId, password);

            if (success)
            {
                System.Windows.MessageBox.Show("회원가입이 완료되었습니다. 로그인해주세요.", "성공", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            else
            {
                System.Windows.MessageBox.Show("이미 존재하는 아이디입니다. 다른 아이디를 사용해주세요.", "실패", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

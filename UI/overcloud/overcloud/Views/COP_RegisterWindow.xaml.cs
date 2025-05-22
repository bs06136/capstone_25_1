using System.Windows;
using DB.overcloud.Repository;

namespace overcloud.Views
{
    public partial class COP_RegisterWindow : Window
    {
        private AccountRepository _accountRepository;
        public COP_RegisterWindow(AccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string userId = IdBox.Text.Trim();
            string password = PasswordBox.Password.Trim();

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(password))
            {
                System.Windows.MessageBox.Show("협업 클라우드 이름과 접근 코드를 모두 입력해주세요.", "입력 오류", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool success = _accountRepository.assign_overcloud(userId, password);

            if (success)
            {
                System.Windows.MessageBox.Show("클라우드 생성이 완료되었습니다. 로그인해주세요.", "성공", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            }
            else
            {
                System.Windows.MessageBox.Show("이미 존재하는 클라우드 이름입니다. 다른 이름을 사용해주세요.", "실패", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

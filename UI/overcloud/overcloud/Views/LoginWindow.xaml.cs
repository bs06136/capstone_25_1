using System.Windows;

namespace overcloud.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // 아이디, 비밀번호는 나중에 사용할 수 있도록 받아두기만 함
            string userId = IdBox.Text;
            string password = PasswordBox.Password;

            // MainWindow 실행
            var main = new MainWindow();
            System.Windows.Application.Current.MainWindow = main;
            main.Show();

            this.Close();
        }
    }
}

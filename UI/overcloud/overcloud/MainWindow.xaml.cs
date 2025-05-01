using System.Windows;
using System.Windows.Input;
using overcloud.Views;

namespace overcloud
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 앱 시작 시 HomeView 로드
            MainFrame.Navigate(new Views.HomeView());
        }

        // 왼쪽 “홈” 메뉴 클릭 시
        private void HomeMenu_Click(object sender, MouseButtonEventArgs e)
        {
            MainFrame.Navigate(new Views.HomeView());
        }

        // 왼쪽 “계정 관리” 메뉴 클릭 시
        private void AccountMenu_Click(object sender, MouseButtonEventArgs e)
        {
            MainFrame.Navigate(new Views.AccountView());

        }
    }
}

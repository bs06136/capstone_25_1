using System.Windows;
using System.Windows.Input;
using DB.overcloud.Repository;
using overcloud.Views;
using OverCloud.Services.FileManager;
using OverCloud.Services.StorageManager;
using OverCloud.Services;

namespace overcloud
{
    public partial class MainWindow : Window
    {

        private readonly LoginController _controller;
        private string _user_id;

        public MainWindow(LoginController controller, string user_id)
        {
            InitializeComponent();
            
            _controller = controller;
            _user_id = user_id;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 앱 시작 시 HomeView 로드
            MainFrame.Navigate(new Views.HomeView(
                _controller.AccountService,
                _controller.FileUploadManager,
                _controller.FileDownloadManager,
                _controller.FileDeleteManager,
                _controller.FileCopyManager,
                _controller.QuotaManager,
                _controller.FileRepository,
                _controller.CloudTierManager,
                _user_id));
        }

        // 왼쪽 “홈” 메뉴 클릭 시
        private void HomeMenu_Click(object sender, MouseButtonEventArgs e)
        {
            MainFrame.Navigate(new Views.HomeView(
                _controller.AccountService,
                _controller.FileUploadManager,
                _controller.FileDownloadManager,
                _controller.FileDeleteManager,
                _controller.FileCopyManager,
                _controller.QuotaManager,
                _controller.FileRepository,
                _controller.CloudTierManager,
                _user_id));
        }

        // 왼쪽 “계정 관리” 메뉴 클릭 시
        private void AccountMenu_Click(object sender, MouseButtonEventArgs e)
        {
            MainFrame.Navigate(new Views.AccountView(
                _controller.AccountService,
                _controller.FileUploadManager,
                _controller.FileDownloadManager,
                _controller.FileDeleteManager,
                _controller.FileCopyManager,
                _controller.QuotaManager,
                _controller.FileRepository,
                _controller.CloudTierManager,
                _user_id));

        }

        private void SharedAccountMenu_Click(object sender, MouseButtonEventArgs e)
        {
            MainFrame.Navigate(new Views.SharedAccountView(
                _controller.AccountService,
                _controller.FileUploadManager,
                _controller.FileDownloadManager,
                _controller.FileDeleteManager,
                _controller.FileCopyManager,
                _controller.QuotaManager,
                _controller.FileRepository,
                _controller.CloudTierManager,
                _user_id,
                _controller.AccountRepository,
                _controller.CooperationManager,
                _controller.CoopUserRepository));
        }

        private void SharedManageMenu_Click(object sender, MouseButtonEventArgs e)
        {
            MainFrame.Navigate(new Views.SharedManageView(
                _controller.AccountService,
                _controller.FileUploadManager,
                _controller.FileDownloadManager,
                _controller.FileDeleteManager,
                _controller.FileCopyManager,
                _controller.QuotaManager,
                _controller.FileRepository,
                _user_id));
        }
    }
}

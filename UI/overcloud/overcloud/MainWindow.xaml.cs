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

        private AccountService _accountService;
        private FileUploadManager _fileUploadManager;
        private FileDownloadManager _fileDownloadManager;
        private FileDeleteManager _fileDeleteManager;
        private FileCopyManager _fileCopyManager;
        private QuotaManager _quotaManager;
        private IFileRepository _fileRepository;
        private CloudTierManager _cloudTierManager;
        private AccountRepository _accountRepository;
        private CooperationManager _CooperationManager;
        private CoopUserRepository _CoopUserRepository;

        private string _user_id;


        public MainWindow(AccountService accountService, FileUploadManager fileUploadManager, FileDownloadManager fileDownloadManager, FileDeleteManager fileDeleteManager, FileCopyManager fileCopyManager, QuotaManager quotaManager, IFileRepository fileRepository, CloudTierManager cloudTierManager, string user_id, AccountRepository accountRepository, CooperationManager CooperationManager , CoopUserRepository coopUserRepository)
        {
            InitializeComponent();
            _accountService = accountService;
            _fileUploadManager = fileUploadManager;
            _fileDownloadManager = fileDownloadManager;
            _fileDeleteManager = fileDeleteManager;
            _fileCopyManager = fileCopyManager;
            _quotaManager = quotaManager;
            _fileRepository = fileRepository;
            _cloudTierManager = cloudTierManager;
            _user_id = user_id;
            _accountRepository = accountRepository;
            _CooperationManager = CooperationManager;
            _CoopUserRepository = coopUserRepository;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 앱 시작 시 HomeView 로드
            MainFrame.Navigate(new Views.HomeView(_accountService, _fileUploadManager, _fileDownloadManager, _fileDeleteManager, _fileCopyManager, _quotaManager, _fileRepository,_cloudTierManager, _user_id));
        }

        // 왼쪽 “홈” 메뉴 클릭 시
        private void HomeMenu_Click(object sender, MouseButtonEventArgs e)
        {
            MainFrame.Navigate(new Views.HomeView(_accountService, _fileUploadManager, _fileDownloadManager, _fileDeleteManager, _fileCopyManager, _quotaManager, _fileRepository, _cloudTierManager, _user_id));
        }

        // 왼쪽 “계정 관리” 메뉴 클릭 시
        private void AccountMenu_Click(object sender, MouseButtonEventArgs e)
        {
            MainFrame.Navigate(new Views.AccountView(_accountService, _fileUploadManager, _fileDownloadManager, _fileDeleteManager, _fileCopyManager, _quotaManager, _fileRepository, _cloudTierManager, _user_id));

        }

        private void SharedAccountMenu_Click(object sender, MouseButtonEventArgs e)
        {
            MainFrame.Navigate(new Views.SharedAccountView(_accountService, _fileUploadManager, _fileDownloadManager, _fileDeleteManager, _fileCopyManager, _quotaManager, _fileRepository, _cloudTierManager, _user_id, _accountRepository, _CooperationManager, _CoopUserRepository));
        }

        private void SharedManageMenu_Click(object sender, MouseButtonEventArgs e)
        {
            MainFrame.Navigate(new Views.SharedManageView(_accountService, _fileUploadManager, _fileDownloadManager, _fileDeleteManager, _fileCopyManager, _quotaManager, _fileRepository, _user_id));
        }
    }
}

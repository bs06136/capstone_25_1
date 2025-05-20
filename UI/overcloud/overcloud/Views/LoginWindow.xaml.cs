using System.Windows;
using DB.overcloud.Repository;
using OverCloud.Services.FileManager;
using OverCloud.Services.StorageManager;
using OverCloud.Services;
using OverCloud.Services.FileManager.DriveManager;
using OverCloud.transfer_manager;

namespace overcloud.Views
{
    public partial class LoginWindow : Window
    {

        private AccountService _accountService;
        private FileUploadManager _fileUploadManager;
        private FileDownloadManager _fileDownloadManager;
        private FileDeleteManager _fileDeleteManager;
        private FileCopyManager _fileCopyManager;
        private QuotaManager _quotaManager;
        private IFileRepository _fileRepository;
        private CloudTierManager _cloudTierManager;

        public LoginWindow()
        {
            InitializeComponent();


            var connStr = DbConfig.ConnectionString;
            var storageRepo = new StorageRepository(connStr);
            var accountRepo = new AccountRepository(connStr);
            _fileRepository = new FileRepository(connStr);

            var tokenFactory = new TokenProviderFactory();
            var googleSvc = new GoogleDriveService(tokenFactory.CreateGoogleTokenProvider(), storageRepo, accountRepo);
            var oneDriveSvc = new OneDriveService(tokenFactory.CreateOneDriveTokenRefresher(), storageRepo, accountRepo);
            var cloudSvcs = new List<ICloudFileService> { googleSvc, oneDriveSvc };

            _quotaManager = new QuotaManager(cloudSvcs, storageRepo, accountRepo);
            _accountService = new AccountService(accountRepo, storageRepo, _quotaManager);
            _cloudTierManager = new CloudTierManager(accountRepo);

            _fileUploadManager = new FileUploadManager(_accountService, _quotaManager, storageRepo, _fileRepository, cloudSvcs, _cloudTierManager);
            _fileDownloadManager = new FileDownloadManager(_fileRepository, accountRepo, cloudSvcs);
            _fileDeleteManager = new FileDeleteManager(accountRepo, _quotaManager, storageRepo, _fileRepository, cloudSvcs);
            _fileCopyManager = new FileCopyManager(_fileRepository, _cloudTierManager, cloudSvcs, _quotaManager, accountRepo, _fileUploadManager);


            // 예: 로그인 성공 후 MainWindow 진입 시
            var storages = accountRepo.GetAllAccounts("admin"); // 현재 로그인된 사용자 기준
            StorageSessionManager.InitializeFromDatabase(storages);
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // 아이디, 비밀번호는 나중에 사용할 수 있도록 받아두기만 함
            string userId = IdBox.Text;
            string password = PasswordBox.Password;
            /*
            string loginResult = login_overcloud(userId, password);

            if (string.IsNullOrEmpty(loginResult))
            {
                System.Windows.MessageBox.Show("아이디 또는 비밀번호가 올바르지 않습니다. 다시 시도해주세요.", "로그인 실패", MessageBoxButton.OK, MessageBoxImage.Warning);
                return; // 창은 닫지 않고 다시 입력 가능
            }

            var storages = new AccountRepository(DbConfig.ConnectionString).GetAllAccounts(userId);
    StorageSessionManager.InitializeFromDatabase(storages);
            */

            App.TransferManager = new TransferManager(_fileUploadManager, _fileDownloadManager);

            // MainWindow 실행
            var main = new MainWindow(_accountService, _fileUploadManager, _fileDownloadManager, _fileDeleteManager, _fileCopyManager, _quotaManager, _fileRepository, _cloudTierManager, user_id);
            System.Windows.Application.Current.MainWindow = main;
            main.Show();

            this.Close();
        }

    }
}

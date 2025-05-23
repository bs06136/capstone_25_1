using System.Windows;
using DB.overcloud.Repository;
using OverCloud.Services.FileManager;
using OverCloud.Services.StorageManager;
using OverCloud.Services;
using OverCloud.Services.FileManager.DriveManager;
using OverCloud.transfer_manager;
using DB.overcloud.Models;
using System.IO;
using System;

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

        private AccountRepository accountRepository;

        private CooperationManager _CooperationManager;
        private CoopUserRepository _CoopUserRepository;

        public LoginWindow()
        {
            InitializeComponent();


            var connStr = DbConfig.ConnectionString;
            var storageRepo = new StorageRepository(connStr);
            //var accountRepo = new AccountRepository(connStr);
            accountRepository = new AccountRepository(DbConfig.ConnectionString);
            _fileRepository = new FileRepository(connStr);

            var tokenFactory = new TokenProviderFactory();
            var googleSvc = new GoogleDriveService(tokenFactory.CreateGoogleTokenProvider(), storageRepo, accountRepository);
            var oneDriveSvc = new OneDriveService(tokenFactory.CreateOneDriveTokenRefresher(), storageRepo, accountRepository);
            var cloudSvcs = new List<ICloudFileService> { googleSvc, oneDriveSvc };

            _quotaManager = new QuotaManager(cloudSvcs, storageRepo, accountRepository);
            _accountService = new AccountService(accountRepository, storageRepo, _quotaManager);
            _cloudTierManager = new CloudTierManager(accountRepository);

            _fileUploadManager = new FileUploadManager(_accountService, _quotaManager, storageRepo, _fileRepository, cloudSvcs, _cloudTierManager);
            _fileDownloadManager = new FileDownloadManager(_fileRepository, accountRepository, cloudSvcs);
            _fileDeleteManager = new FileDeleteManager(accountRepository, _quotaManager, storageRepo, _fileRepository, cloudSvcs);
            _fileCopyManager = new FileCopyManager(_fileRepository, _cloudTierManager, cloudSvcs, _quotaManager, accountRepository, _fileUploadManager);

        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // 아이디, 비밀번호는 나중에 사용할 수 있도록 받아두기만 함
            string userId = IdBox.Text;
            string password = PasswordBox.Password;
            
            string loginResult = accountRepository.login_overcloud(userId, password);

            if (string.IsNullOrEmpty(loginResult))
            {
                System.Windows.MessageBox.Show("아이디 또는 비밀번호가 올바르지 않습니다. 다시 시도해주세요.", "로그인 실패", MessageBoxButton.OK, MessageBoxImage.Warning);
                return; // 창은 닫지 않고 다시 입력 가능
            }

            //var storages = new AccountRepository(DbConfig.ConnectionString).GetAllAccounts(userId);
            //StorageSessionManager.InitializeFromDatabase(storages);

            // 1. 계정 리스트 구성
            var allAccounts = new List<string> { userId };
            allAccounts.AddRange(_CoopUserRepository.connected_cooperation_account_nums(userId));

            // 2. 전체 스토리지 수집
            var allStorages = new List<CloudStorageInfo>();
            foreach (var accId in allAccounts.Distinct())
            {
                var storages = accountRepository.GetAllAccounts(accId);
                allStorages.AddRange(storages);
            }

            // 3. 세션 초기화
            StorageSessionManager.InitializeFromDatabase(allStorages);


            App.TransferManager = new TransferManager(_fileUploadManager, _fileDownloadManager, _cloudTierManager);

            // MainWindow 실행
            var main = new MainWindow(_accountService, _fileUploadManager, _fileDownloadManager, _fileDeleteManager, _fileCopyManager, _quotaManager, _fileRepository, _cloudTierManager, userId, accountRepository, _CooperationManager, _CoopUserRepository);
            System.Windows.Application.Current.MainWindow = main;
            main.Show();

            this.Close();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            // 예시: 회원가입 창 띄우기
            var registerWindow = new RegisterWindow(accountRepository); // 따로 만들어진 회원가입 창
            registerWindow.Owner = this;
            registerWindow.ShowDialog();
        }

    }
}

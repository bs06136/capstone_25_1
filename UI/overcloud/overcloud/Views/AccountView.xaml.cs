using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using OverCloud.Services.FileManager;
using OverCloud.Services.StorageManager;
using OverCloud.Services;
using DB.overcloud.Repository;

namespace overcloud.Views
{
    public partial class AccountView : System.Windows.Controls.UserControl
    {
        // 서비스 초기화

        private readonly AccountService _accountService;
        private readonly FileUploadManager _fileUploadManager;
        private readonly FileDownloadManager _fileDownloadManager;
        private readonly FileDeleteManager _fileDeleteManager;
        private readonly FileCopyManager _fileCopyManager;
        private readonly QuotaManager _quotaManager;
        private readonly IFileRepository _fileRepository;
        private readonly CloudTierManager _cloudTierManager;
        private string _user_id;

        public AccountView(AccountService accountService, FileUploadManager fileUploadManager, FileDownloadManager fileDownloadManager, FileDeleteManager fileDeleteManager, FileCopyManager fileCopyManager, QuotaManager quotaManager, IFileRepository fileRepository, CloudTierManager cloudTierManager, string user_id)
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

            // 최초 로드 시 “계정 관리” 목록 화면으로
            SubFrame.Navigate(new AccountListView(_accountService, _fileUploadManager, _fileDownloadManager, _fileDeleteManager, _fileCopyManager, _quotaManager, _fileRepository, _user_id));
        }

        private void AccountMenu_Click(object sender, MouseButtonEventArgs e)
        {
            SubFrame.Navigate(new AccountListView(_accountService, _fileUploadManager, _fileDownloadManager, _fileDeleteManager, _fileCopyManager, _quotaManager, _fileRepository, _user_id));
        }

        private void DetailMenu_Click(object sender, MouseButtonEventArgs e)
        {
            SubFrame.Navigate(new AccountDetailView(_accountService, _fileUploadManager, _fileDownloadManager, _fileDeleteManager, _fileCopyManager, _quotaManager, _fileRepository, _user_id));
        }
    }
}

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using OverCloud.Services.FileManager;
using OverCloud.Services.StorageManager;
using OverCloud.Services;
using DB.overcloud.Repository;
using overcloud.Views;

namespace overcloud.Views
{
    public partial class SharedManageView : System.Windows.Controls.UserControl
    {
        private readonly AccountService _accountService;
        private readonly FileUploadManager _fileUploadManager;
        private readonly FileDownloadManager _fileDownloadManager;
        private readonly FileDeleteManager _fileDeleteManager;
        private readonly FileCopyManager _fileCopyManager;
        private readonly QuotaManager _quotaManager;
        private readonly IFileRepository _fileRepository;
        private string _user_id;

        public SharedManageView(AccountService accountService, FileUploadManager fileUploadManager, FileDownloadManager fileDownloadManager, FileDeleteManager fileDeleteManager, FileCopyManager fileCopyManager, QuotaManager quotaManager, IFileRepository fileRepository, string user_id)
        {
            InitializeComponent();
            _accountService = accountService;
            _fileUploadManager = fileUploadManager;
            _fileDownloadManager = fileDownloadManager;
            _fileDeleteManager = fileDeleteManager;
            _fileCopyManager = fileCopyManager;
            _quotaManager = quotaManager;
            _fileRepository = fileRepository;
            _user_id = user_id;

            // 최초 로드 시 “공유 계정” 목록 화면으로
            SubFrame.Navigate(new SharedAccountListView(_accountService, _fileUploadManager, _fileDownloadManager, _fileDeleteManager, _fileCopyManager, _quotaManager, _fileRepository, _user_id));
        }

        private void SharedManageMenu_Click(object sender, MouseButtonEventArgs e)
        {
            SubFrame.Navigate(new SharedAccountListView(_accountService, _fileUploadManager, _fileDownloadManager, _fileDeleteManager, _fileCopyManager, _quotaManager, _fileRepository, _user_id));
        }

        private void SharedDetailMenu_Click(object sender, MouseButtonEventArgs e)
        {
            SubFrame.Navigate(new SharedAccountDetailView(_accountService, _fileUploadManager, _fileDownloadManager, _fileDeleteManager, _fileCopyManager, _quotaManager, _fileRepository, _user_id));
        }
    }
}

using System.Windows;
using System.Windows.Input;
using DB.overcloud.Repository;
using overcloud.Views;
using OverCloud.Services.FileManager;
using OverCloud.Services.StorageManager;
using OverCloud.Services;
using OverCloud.transfer_manager;
using overcloud.Windows;

namespace overcloud
{
    public partial class MainWindow : Window
    {

        private LoginController _controller;
        private string _user_id;

        private TransferManagerWindow _transferWindow;

        public MainWindow(LoginController controller, string user_id)
        {
            InitializeComponent();
            
            _controller = controller;
            _user_id = user_id;

            App.DownloadRequestReceived += OnDownloadRequestReceived;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 앱 시작 시 HomeView 로드
            MainFrame.Navigate(new Views.HomeView(
                _controller,
                _user_id));

            if (App.PendingDownloads.Count > 0)
            {
                EnqueueDownloadsFromPending();
            }
        }

        // 왼쪽 “홈” 메뉴 클릭 시
        private void HomeMenu_Click(object sender, MouseButtonEventArgs e)
        {
            MainFrame.Navigate(new Views.HomeView(
                _controller,
                _user_id));
        }

        // 왼쪽 “계정 관리” 메뉴 클릭 시
        private void AccountMenu_Click(object sender, MouseButtonEventArgs e)
        {
            MainFrame.Navigate(new Views.AccountView(
                _controller,
                _user_id));

        }

        private void SharedAccountMenu_Click(object sender, MouseButtonEventArgs e)
        {
            MainFrame.Navigate(new Views.SharedAccountView(
                _controller,
                _user_id));
        }

        private void SharedManageMenu_Click(object sender, MouseButtonEventArgs e)
        {
            MainFrame.Navigate(new Views.SharedManageView(
                _controller,
                _user_id));
        }

        private void EnqueueDownloadsFromPending()
        {
            List<(int FileID, string FileName, string CloudFileId, int CloudStorageNum, string LocalPath, bool IsDistributed, ulong FileSize)> downloadList = new();

            foreach (var item in App.PendingDownloads)
            {
                try
                {
                    var fileInfo = _controller.FileRepository.specific_file_info(item.fileId);
                    if (fileInfo != null)
                    {
                        string savePath = ShowSaveFolderDialog(fileInfo.FileName);
                        if (string.IsNullOrEmpty(savePath))
                            continue;

                        downloadList.Add((
                            FileID: fileInfo.FileId,
                            FileName: fileInfo.FileName,
                            CloudFileId: fileInfo.CloudFileId,
                            CloudStorageNum: fileInfo.CloudStorageNum,
                            LocalPath: savePath,
                            IsDistributed: fileInfo.IsDistributed,
                            FileSize: fileInfo.FileSize
                        ));
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"파일 조회 실패: {ex.Message}");
                }
            }

            if (downloadList.Count > 0)
            {
                App.TransferManager.DownloadManager.EnqueueDownloads(downloadList, App.PendingDownloads[0].userId);
                App.PendingDownloads.Clear();
                ShowTransferWindow();
            }
        }

        private void ShowTransferWindow()
        {
            if (_transferWindow == null || !_transferWindow.IsVisible)
            {
                _transferWindow = new TransferManagerWindow();
                _transferWindow.Owner = System.Windows.Application.Current.MainWindow;          
                _transferWindow.Show();
            }
            else
            {
                _transferWindow.Activate();
            }
        }

        private string ShowSaveFolderDialog(string defaultFileName)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog();
            dialog.FileName = defaultFileName;
            dialog.Filter = "모든 파일 (*.*)|*.*";

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                return dialog.FileName;
            }
            else
            {
                return null;
            }
        }

        private void OnDownloadRequestReceived()
        {
            // Pipe로 새 요청이 왔을 때만 호출
            if (App.PendingDownloads.Count > 0)
            {
                EnqueueDownloadsFromPending();
            }
        }

        private void IssueManageMenu_Click(object sender, MouseButtonEventArgs e)
        {
            MainFrame.Navigate(new Views.IssueManageView(_controller, _user_id));
        }

    }
}

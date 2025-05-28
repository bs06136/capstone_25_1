using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using DB.overcloud.Models;
using overcloud.transfer_manager;
using OverCloud.Services;

namespace overcloud.Windows
{
    public partial class DownloadFromLinkWindow : Window
    {
        private string _currentUserId;
        private LoginController _controller;

        public DownloadFromLinkWindow(string userId, LoginController controller)
        {
            InitializeComponent();
            _currentUserId = userId;
            _controller = controller;

            // 클립보드에 텍스트 있으면 미리 채움
            if (System.Windows.Clipboard.ContainsText())
                LinkTextBox.Text = System.Windows.Clipboard.GetText();
        }

        private async void Download_Click(object sender, RoutedEventArgs e)
        {
            string input = LinkTextBox.Text.Trim();
            if (string.IsNullOrEmpty(input))
            {
                System.Windows.MessageBox.Show("링크를 입력해주세요.");
                return;
            }

            var folderDialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "파일을 저장할 폴더를 선택하세요.",
                RootFolder = Environment.SpecialFolder.MyComputer
            };
            if (folderDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            string basePath = folderDialog.SelectedPath;

            // ✅ 전송 관리자 창 띄우기
            if (System.Windows.Application.Current.MainWindow is MainWindow mainWin)
            {
                var transferWindow = new TransferManagerWindow();
                transferWindow.Owner = mainWin;
                transferWindow.Show();
            }

            var parts = input.Split('|');

            foreach (var part in parts)
            {
                var tokens = part.Split(',');
                if (tokens.Length != 3) continue;

                string userId = tokens[0];
                string cloudFileId = tokens[1];
                if (!int.TryParse(tokens[2], out int fileId)) continue;

                await DownloadRecursive(fileId, userId, basePath);
            }

            System.Windows.MessageBox.Show("다운로드 요청 완료");
            Close();
        }


        private async Task DownloadRecursive(int fileId, string userId, string localBase)
        {
            var file = _controller.FileRepository.specific_file_info(fileId);
            if (file == null) return;

            string localPath = Path.Combine(localBase, file.FileName);

            if (file.IsFolder || string.IsNullOrEmpty(file.CloudFileId))
            {
                Directory.CreateDirectory(localPath);
                var children = _controller.FileRepository.all_file_list(file.FileId, userId);
                foreach (var child in children)
                {
                    await DownloadRecursive(child.FileId, userId, localPath);
                }
            }
            else
            {
                string? dir = Path.GetDirectoryName(localPath);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

                App.TransferManager.DownloadManager.EnqueueDownloads(
                    new List<(int FileId, string FileName, string CloudFileId, int CloudStorageNum, string LocalPath, bool IsDistributed)>
                    {
                (file.FileId, file.FileName, file.CloudFileId, file.CloudStorageNum, localPath, file.IsDistributed)
                    },
                    _currentUserId);
            }
        }



    }
}

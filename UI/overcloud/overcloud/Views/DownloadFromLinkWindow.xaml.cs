using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DB.overcloud.Models;
using overcloud.transfer_manager;
using OverCloud.Services;
using SourceChord.FluentWPF;

namespace overcloud.Windows
{
    public partial class DownloadFromLinkWindow : AcrylicWindow
    {
        private readonly string _currentUserId;
        private readonly LoginController _controller;

        public DownloadFromLinkWindow(string userId, LoginController controller)
        {
            InitializeComponent();
            _currentUserId = userId;
            _controller = controller;

            // ① 클립보드에 URL/파라미터가 있으면 미리 채움
            if (System.Windows.Clipboard.ContainsText())
                LinkTextBox.Text = System.Windows.Clipboard.GetText();
        }

        private async void Download_Click(object sender, RoutedEventArgs e)
        {
            // 1) 원본 입력값 가져오기
            string raw = LinkTextBox.Text.Trim();
            if (string.IsNullOrEmpty(raw))
            {
                System.Windows.MessageBox.Show("링크를 입력해주세요.");
                return;
            }

            // 2) HTTP/HTTPS URL 형태라면 쿼리에서 link 파라미터만 꺼내기
            string input = raw;
            if (Uri.TryCreate(raw, UriKind.Absolute, out var uri) &&
                (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                // 예: "?link=admin1%2C...%2C400087"
                var qs = uri.Query
                            .TrimStart('?')
                            .Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);

                string linkParam = qs
                    .Select(part => part.Split(new[] { '=' }, 2))
                    .Where(kv => kv.Length == 2 && kv[0] == "link")
                    .Select(kv => Uri.UnescapeDataString(kv[1]))
                    .FirstOrDefault();

                if (string.IsNullOrEmpty(linkParam))
                {
                    System.Windows.MessageBox.Show("유효한 link 파라미터가 없습니다.");
                    return;
                }

                input = linkParam;
            }

            // 3) 쉼표로 userId, cloudFileId, fileId 분리
            var tokens = input.Split(',');
            if (tokens.Length != 3)
            {
                System.Windows.MessageBox.Show("링크 형식이 올바르지 않습니다.\n예: userId,cloudFileId,fileId");
                return;
            }

            string userId = tokens[0];
            string cloudFileId = tokens[1];
            if (!int.TryParse(tokens[2], out int fileId))
            {
                System.Windows.MessageBox.Show("파일 ID가 유효하지 않습니다.");
                return;
            }

            // 4) 저장 폴더 선택
            var folderDlg = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "파일을 저장할 폴더를 선택하세요.",
                RootFolder = Environment.SpecialFolder.MyComputer
            };
            if (folderDlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;
            string basePath = folderDlg.SelectedPath;

            // 5) 전송 관리자 창 띄우기
            if (System.Windows.Application.Current.MainWindow is MainWindow mainWin)
            {
                var transferWin = new TransferManagerWindow { Owner = mainWin };
                transferWin.Show();
            }

            // 6) 실제 다운로드 요청
            await DownloadRecursive(fileId, userId, basePath);
            Console.WriteLine($"다운로드 요청: userId={userId}, fileId={fileId}, basePath={basePath}");

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
                    await DownloadRecursive(child.FileId, userId, localPath);
            }
            else
            {
                var dir = Path.GetDirectoryName(localPath);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

                App.TransferManager.DownloadManager.EnqueueDownloads(
                    new List<(int FileId, string FileName, string CloudFileId, int CloudStorageNum, string LocalPath, bool IsDistributed, ulong FileSize)>
                    {
                        (file.FileId, file.FileName, file.CloudFileId, file.CloudStorageNum, localPath, file.IsDistributed, file.FileSize)
                    },
                    _currentUserId);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

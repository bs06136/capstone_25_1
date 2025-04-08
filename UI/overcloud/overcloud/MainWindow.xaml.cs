using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using overcloud.Views;
using DB.overcloud.Models;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows.Forms;
using static overcloud.temp_class.TempClass;
using OverCloud.Services;
using DB.overcloud.Repository;
using overcloud.Converters;

namespace overcloud
{
    public partial class MainWindow : Window
    {

        private AccountService _accountService;
        private FileUploadManager _FileUploadManager;
        private StorageUpdater _storageUpdater;
        private FileDownloadManager _fileDownloadManager;

        private int currentFolderId = -1; // 현재 폴더 위치
        private Stack<int> folderHistory ; // 이전 폴더 기억용
        private Dictionary<int, bool> selectedMap;  // 2번째 탐색기에서 체크박스 상태 기억용
        private IFileRepository FileRepository; // 전체 파일 목록   



        public MainWindow()
        {
            InitializeComponent();

            _accountService = new AccountService();
            _FileUploadManager = new FileUploadManager();

            _storageUpdater = new StorageUpdater();  // 수정 필요
            SaveDriveQuotaToDBAsync();  // 수정 필요

            _fileDownloadManager = new FileDownloadManager();
            FileRepository = new FileRepository(DbConfig.ConnectionString);
        }

        private async void SaveDriveQuotaToDBAsync()
        {
            await _storageUpdater.SaveDriveQuotaToDB("bszxcvbn@gmail.com", 1);
        }

        private void Button_Add_Click(object sender, RoutedEventArgs e)
        {
            AddAccountWindow window = new AddAccountWindow(_accountService);
            window.ShowDialog();
        }

        private void Button_Delete_Click(object sender, RoutedEventArgs e)
        {
            DeleteAccountWindow window = new DeleteAccountWindow(_accountService);
            window.Owner = this;
            window.ShowDialog();
        }


        private async void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            var choice = System.Windows.MessageBox.Show(
                "파일을 선택하려면 [예], 폴더를 선택하려면 [아니오]를 클릭하세요.",
                "선택 방식",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            if (choice == MessageBoxResult.Yes)
            {
                // 파일 선택
                var fileDialog = new CommonOpenFileDialog()
                {
                    IsFolderPicker = false,
                    Multiselect = false,
                    Title = "파일 선택"
                };

                if (fileDialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    string filePath = fileDialog.FileName;

                    // ⭐ temp_class.file_upload 호출
                    bool result = await _FileUploadManager.file_upload(filePath);

                    System.Windows.MessageBox.Show(result
                        ? $"파일 업로드 성공\n경로: {filePath}"
                        : "파일 업로드 실패");
                }
            }
            else if (choice == MessageBoxResult.No)
            {
                // 폴더 선택
                using (var folderDialog = new FolderBrowserDialog())
                {
                    folderDialog.Description = "폴더 선택";
                    folderDialog.RootFolder = System.Environment.SpecialFolder.MyComputer;

                    if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        string folderPath = folderDialog.SelectedPath;

                        // ⭐ temp_class.file_upload 호출
                        bool result = true;      //file_upload(folderPath);

                        System.Windows.MessageBox.Show(result
                            ? $"폴더 업로드 성공\n경로: {folderPath}"
                            : "폴더 업로드 실패");
                    }
                }
            }
        }

        private void Button_DetailDisk_Click(object sender, RoutedEventArgs e)
        {
            DiskDetailWindow detailWindow = new DiskDetailWindow(_accountService);
            detailWindow.Owner = this;
            detailWindow.ShowDialog();
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadRootFolders();
        }

        private void LoadRootFolders()
        {
            //var rootItems = all_file_list(-1); // ParentFolderId == null 처리
            var rootItems = FileRepository.all_file_list(null);
            foreach (var item in rootItems)
            {
                var node = new FileTreeNode(item);
                node.Expanded += FileNode_Expanded;
                FileExplorerTree.Items.Add(node);
            }
        }

        private void FileNode_Expanded(object sender, RoutedEventArgs e)
        {
            if (sender is FileTreeNode node && node.FileInfo.IsFolder && !node.IsLoaded)
            {
                var children = FileRepository.all_file_list(node.FileInfo.FileId);
                node.LoadChildren(children);

                // 자식에도 이벤트 달기
                foreach (var child in node.Items)
                {
                    if (child is FileTreeNode childNode && childNode.FileInfo.IsFolder)
                        childNode.Expanded += FileNode_Expanded;
                }
            }
        }


        private List<CloudFileInfo> GetCheckedFiles()
        {
            if (FileExplorerTree.Visibility == Visibility.Visible)
            {
                var result = new List<CloudFileInfo>();
                foreach (var item in FileExplorerTree.Items)
                {
                    if (item is FileTreeNode node)
                        FindCheckedFilesRecursive(node, result, false);
                }
                return result;
            }
            else
            {
                return GetCheckedFiles_NewExplorer();  // ⭐ 이거 추가!
            }
        }

        private void FindCheckedFilesRecursive(FileTreeNode node, List<CloudFileInfo> result, bool parentChecked = false)
        {
            bool isCurrentChecked = node.IsChecked || parentChecked;

            if (node.FileInfo.IsFolder)
            {
                if (!node.IsLoaded)
                {
                    var children = FileRepository.all_file_list(node.FileInfo.FileId);
                    node.LoadChildren(children);
                }

                foreach (var child in node.ChildrenNodes)
                {
                    FindCheckedFilesRecursive(child, result, isCurrentChecked);
                }
            }
            else
            {
                if (isCurrentChecked)
                    result.Add(node.FileInfo);
            }
        }

        
        private async void Button_Down_Click(object sender, RoutedEventArgs e)
        {
            var selectedFiles = GetCheckedFiles();
            if (selectedFiles.Count == 0)
            {
                System.Windows.MessageBox.Show("선택된 파일이 없습니다.");
                return;
            }

            // 전체 CloudFileInfo를 ID 기준으로 저장 (부모 경로 추적용)
            Dictionary<int, CloudFileInfo> allFileMap = GetAllFilesRecursively();

            // 기본 저장 루트 (예시)
            string localBase = @"C:\down";

            foreach (var file in selectedFiles)
            {
                string relativePath = GetCloudPath(file, allFileMap); // Cloud 경로
                string localPath = System.IO.Path.Combine(localBase, relativePath);

                // 폴더 생성
                string? dir = System.IO.Path.GetDirectoryName(localPath);
                if (!string.IsNullOrEmpty(dir)) System.IO.Directory.CreateDirectory(dir);

                // 실제 다운로드 (예: 구글드라이브에서 파일 가져오기)
                await _fileDownloadManager.DownloadFile("1",  file.GoogleFileId,  file.FileId, localPath);
            }

            System.Windows.MessageBox.Show("다운로드 완료");
        }


        /*
        private void Button_Down_Click(object sender, RoutedEventArgs e)    //testcode
        {
            var selectedFiles = GetCheckedFiles();

            if (selectedFiles.Count == 0)
            {
                System.Windows.MessageBox.Show("선택된 파일이 없습니다.");
                return;
            }

            string message = "선택된 파일:\n" + string.Join("\n", selectedFiles.Select(f => f.FileName));
            System.Windows.MessageBox.Show(message, "파일 선택 결과");
        }*/

        private string GetCloudPath(CloudFileInfo file, Dictionary<int, CloudFileInfo> allMap)
        {
            List<string> pathParts = new List<string> { file.FileName };
            int? currentParent = file.ParentFolderId;

            while (currentParent.HasValue && allMap.ContainsKey(currentParent.Value))
            {
                var parent = allMap[currentParent.Value];
                pathParts.Insert(0, parent.FileName);
                currentParent = parent.ParentFolderId;
            }

            return string.Join("\\", pathParts);
        }

        private Dictionary<int, CloudFileInfo> GetAllFilesRecursively()
        {
            var result = new Dictionary<int, CloudFileInfo>();
            void Traverse(int fileId)
            {
                var children = FileRepository.all_file_list(fileId);
                foreach (var item in children)
                {
                    result[item.FileId] = item;
                    if (item.IsFolder) Traverse(item.FileId);
                }
            }
            Traverse(-1);
            return result;
        }





        private void Button_SwitchExplorer_Click(object sender, RoutedEventArgs e)
        {
            bool isTreeVisible = FileExplorerTree.Visibility == Visibility.Visible;

            FileExplorerTree.Visibility = isTreeVisible ? Visibility.Collapsed : Visibility.Visible;
            Panel_FolderExplorer.Visibility = isTreeVisible ? Visibility.Visible : Visibility.Collapsed;

            if (!isTreeVisible) return;

            currentFolderId = -1;
            LoadFolderContents(currentFolderId);
        }

        private void FolderItem_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock tb && tb.DataContext is CloudFileInfo info && info.IsFolder)
            {
                folderHistory.Push(currentFolderId);
                currentFolderId = info.FileId;
                LoadFolderContents(currentFolderId);
            }
        }


        private void LoadFolderContents(int folderId)
        {
            var contents = FileRepository.all_file_list(folderId).ToList();

            // 현재 폴더의 부모를 알아내서 "상위 폴더로" 항목 삽입
            if (folderId != -1)
            {
                var all = GetAllFilesRecursively();
                if (all.TryGetValue(folderId, out var current) && current.ParentFolderId.HasValue)
                {
                    contents.Insert(0, new CloudFileInfo
                    {
                        FileName = "📁 상위 폴더로",
                        IsFolder = true,
                        FileId = current.ParentFolderId.Value
                    });
                }
            }

            FolderContentPanel.ItemsSource = contents;
        }


        private string GetCloudPathString(int folderId)
        {
            if (folderId == -1) return "Root";

            Dictionary<int, CloudFileInfo> allMap = GetAllFilesRecursively();  // 전체 트리

            List<string> parts = new();
            int? current = folderId;

            while (current.HasValue && allMap.ContainsKey(current.Value))
            {
                parts.Insert(0, allMap[current.Value].FileName);
                current = allMap[current.Value].ParentFolderId;
            }

            return "Root > " + string.Join(" > ", parts);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.CheckBox cb && cb.Tag is int id)
                selectedMap[id] = true;
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.CheckBox cb && cb.Tag is int id)
                selectedMap[id] = false;
        }


        private List<CloudFileInfo> GetCheckedFiles_NewExplorer()
        {
            if (FolderContentPanel.ItemsSource is IEnumerable<CloudFileInfo> list)
            {
                return list
                    .Where(f => selectedMap.TryGetValue(f.FileId, out var isChecked)
                                && isChecked
                                && f.FileName != "📁 상위 폴더로")
                    .ToList();
            }

            return new List<CloudFileInfo>();
        }





    }
}
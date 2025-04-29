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
        private Stack<int> folderHistory; // 이전 폴더 기억용
        private Dictionary<int, bool> selectedMap;  // 2번째 탐색기에서 체크박스 상태 기억용
        private IFileRepository FileRepository; // 전체 파일 목록   
        private FileDeleteManager fileDeleteManager = new();



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
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadRootFolders();
            RefreshExplorer();
        }

        private async void SaveDriveQuotaToDBAsync()
        {
            await _storageUpdater.SaveDriveQuotaToDB("bszxcvbn@gmail.com", 1);
        }

        private void Button_Add_Click(object sender, RoutedEventArgs e)
        {
            AddAccountWindow window = new AddAccountWindow(_accountService);
            window.ShowDialog();
            RefreshExplorer();
        }

        private void Button_Delete_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("삭제 버튼 누름");
            DeleteAccountWindow window = new DeleteAccountWindow(_accountService);
            window.Owner = this;
            window.ShowDialog();
            //RefreshExplorer();
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

        //FileRepository.all_file_list

        private void LoadRootFolders()
        {
            // "모든 파일" 루트 노드
            var rootItem = new TreeViewItem
            {
                Header = "최상위 폴더",
                Tag = -1
            };

            // 바로 하위 폴더만 조회해서 추가
            var rootChildren = all_file_list(-1)
                                 .Where(f => f.IsFolder)
                                 .ToList();

            foreach (var child in rootChildren)
            {
                var childItem = new TreeViewItem
                {
                    Header = child.FileName,
                    Tag = child.FileId
                };
                childItem.Items.Add("Loading..."); // 하위 폴더 열 때만 로드
                childItem.Expanded += Folder_Expanded;
                rootItem.Items.Add(childItem);
            }

            FileExplorerTree.Items.Add(rootItem);
        }

        private void Folder_Expanded(object sender, RoutedEventArgs e)
        {
            if (sender is TreeViewItem parentItem)
            {
                if (parentItem.Items.Count == 1 && parentItem.Items[0] is string && (string)parentItem.Items[0] == "Loading...")
                {
                    parentItem.Items.Clear();

                    int parentId = (int)parentItem.Tag;

                    var children = all_file_list(parentId)
                                    .Where(f => f.IsFolder)
                                    .ToList();

                    foreach (var child in children)
                    {
                        var childItem = new TreeViewItem
                        {
                            Header = child.FileName,
                            Tag = child.FileId
                        };
                        childItem.Items.Add("Loading..."); // 또 하위가 있을 수 있으니
                        childItem.Expanded += Folder_Expanded;
                        parentItem.Items.Add(childItem);
                    }
                }
            }
        }

        private void RefreshExplorer()
        {
            // 1. 현재 열려 있는 노드들의 ID 저장
            var expandedIds = new HashSet<int>();
            foreach (var item in FileExplorerTree.Items)
            {
                if (item is FileTreeNode node)
                    CollectExpandedNodes(node, expandedIds);
            }

            // 2. 트리 전체 클리어
            FileExplorerTree.Items.Clear();

            // 3. 루트부터 다시 로드
            LoadRootFolders();

            // 4. 저장한 ID 기준으로 다시 열기
            ExpandNodesById(FileExplorerTree.Items, expandedIds);
        }

        private void CollectExpandedNodes(FileTreeNode node, HashSet<int> expandedIds)
        {
            if (node.IsExpanded)
                expandedIds.Add(node.FileInfo.FileId);

            foreach (var child in node.Items)
            {
                if (child is FileTreeNode childNode)
                    CollectExpandedNodes(childNode, expandedIds);
            }
        }

        private void ExpandNodesById(ItemCollection items, HashSet<int> expandedIds)
        {
            foreach (var item in items)
            {
                if (item is FileTreeNode node)
                {
                    if (expandedIds.Contains(node.FileInfo.FileId))
                    {
                        node.IsExpanded = true;
                        if (!node.IsLoaded)
                        {
                            var children = all_file_list(node.FileInfo.FileId);
                            node.LoadChildren(children);

                            // 자식에도 이벤트 연결
                            foreach (var child in node.Items)
                            {
                                if (child is FileTreeNode childNode && childNode.FileInfo.IsFolder)
                                    childNode.Expanded += FileNode_Expanded;
                            }
                        }
                        ExpandNodesById(node.Items, expandedIds); // 재귀적으로 하위도 펼치기
                    }
                }
            }
        }


        private void FileNode_Expanded(object sender, RoutedEventArgs e)
        {
            if (sender is FileTreeNode node && node.FileInfo.IsFolder && !node.IsLoaded)
            {
                var children = all_file_list(node.FileInfo.FileId);
                node.LoadChildren(children);

                // 자식에도 이벤트 달아주기 (다시 확장할 수 있게)
                foreach (var child in node.Items)
                {
                    if (child is FileTreeNode childNode && childNode.FileInfo.IsFolder)
                    {
                        childNode.Expanded += FileNode_Expanded;
                    }
                }
            }
        }

        private void FileExplorerTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is TreeViewItem item && item.Tag is int folderId)
            {
                currentFolderId = folderId;
                LoadFolderContents(currentFolderId);
            }
        }

        private void LoadFolderContents(int folderId)
        {
            var contents = all_file_list(folderId).Select(file =>
            {
                return new
                {
                    FileName = file.FileName,
                    Icon = file.IsFolder ? "asset/folder.png" : "asset/file.png"  // 폴더는 폴더아이콘, 파일은 파일아이콘
                };
            }).ToList();

            RightFileListPanel.ItemsSource = contents;
        }


        private void RightFileItem_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is StackPanel panel && panel.DataContext != null)
            {
                var fileInfo = panel.DataContext;

                // dynamic으로 분리
                dynamic info = fileInfo;

                string fileName = info.FileName;
                string iconPath = info.Icon;

                if (iconPath == "asset/folder.png")
                {
                    var folder = all_file_list(currentFolderId)
                                 .FirstOrDefault(f => f.IsFolder && f.FileName == fileName);

                    if (folder != null)
                    {
                        currentFolderId = folder.FileId;
                        LoadFolderContents(currentFolderId);
                        SelectFolderInTree(folder.FileId);
                    }
                }
            }
        }




        private void SelectFolderInTree(int folderId)
        {
            foreach (var item in FileExplorerTree.Items)
            {
                if (item is TreeViewItem rootItem)
                {
                    if (SelectFolderInTreeRecursive(rootItem, folderId))
                        break;
                }
            }
        }

        private bool SelectFolderInTreeRecursive(TreeViewItem parent, int folderId)
        {
            if (parent.Tag is int id && id == folderId)
            {
                parent.IsSelected = true;
                parent.BringIntoView();
                return true;
            }

            foreach (var childObj in parent.Items)
            {
                if (childObj is TreeViewItem childItem)
                {
                    // 하위 항목이 "Loading..."이고 아직 로드되지 않은 경우만 처리
                    if (childItem.Items.Count == 1 && childItem.Items[0] is string s && s == "Loading...")
                    {
                        // 여기서 childItem.Tag 기준으로 로드해야 함!
                        if (childItem.Tag is int childId)
                        {
                            childItem.Items.Clear();

                            // ⚠️ 하위 항목을 중복해서 추가하지 않도록 체크
                            var children = all_file_list(childId)
                                           .Where(f => f.IsFolder && f.FileId != childId) // 자기 자신은 제외
                                           .ToList();

                            foreach (var child in children)
                            {
                                // 중복 방지: 같은 FileId의 항목이 이미 존재하면 추가하지 않음
                                bool alreadyExists = childItem.Items.OfType<TreeViewItem>()
                                                      .Any(x => x.Tag is int tag && tag == child.FileId);
                                if (alreadyExists) continue;

                                var newChild = new TreeViewItem
                                {
                                    Header = child.FileName,
                                    Tag = child.FileId
                                };
                                newChild.Items.Add("Loading...");
                                newChild.Expanded += Folder_Expanded;
                                childItem.Items.Add(newChild);
                            }
                        }
                    }

                    if (SelectFolderInTreeRecursive(childItem, folderId))
                        return true;
                }
            }

            return false;
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true; // 클릭이 상위 StackPanel로 가지 않게 막음
        }
    }
}
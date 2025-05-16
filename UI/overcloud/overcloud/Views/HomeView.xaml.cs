using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;
using DB.overcloud.Models;
using DB.overcloud.Repository;
using OverCloud.Services;
using OverCloud.Services.FileManager;
using OverCloud.Services.FileManager.DriveManager;
using OverCloud.Services.StorageManager;
using overcloud.Converters;
//using static overcloud.temp_class.TempClass;

namespace overcloud.Views
{
    public partial class HomeView : System.Windows.Controls.UserControl
    {

        private AccountService _accountService;
        private FileUploadManager _fileUploadManager;
        private FileDownloadManager _fileDownloadManager;
        private FileDeleteManager _fileDeleteManager;
        private FileCopyManager _fileCopyManager;
        private QuotaManager _quotaManager;
        private IFileRepository _fileRepository;


        // 탐색기 상태
        private int currentFolderId = -1;
        private bool isMoveMode = false;
        private int moveTargetFolderId = -2;
        private List<FileItemViewModel> moveCandidates = new();

        public HomeView(AccountService accountService, FileUploadManager fileUploadManager, FileDownloadManager fileDownloadManager, FileDeleteManager fileDeleteManager, FileCopyManager fileCopyManager, QuotaManager quotaManager, IFileRepository fileRepository)
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString(), "XAML 파싱 에러");
                throw;
            }
            Loaded += HomeView_Loaded;
            _accountService = accountService;
            _fileUploadManager = fileUploadManager;
            _fileDownloadManager = fileDownloadManager;
            _fileDeleteManager = fileDeleteManager;
            _fileCopyManager = fileCopyManager;
            _quotaManager = quotaManager;
            _fileRepository = fileRepository;

            // 초기 서비스 설정
        }


        private void HomeView_Loaded(object sender, RoutedEventArgs e)
        {
            LoadRootFolders();
            RefreshExplorer();
        }


        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///탐색기 참조를 위한 클래스
        ///
        public class FileItemViewModel : INotifyPropertyChanged
        {
            public int FileId { get; set; }
            public string FileName { get; set; }
            public ulong FileSize { get; set; }
            public DateTime UploadedAt { get; set; }
            public int CloudStorageNum { get; set; }
            public int? ParentFolderId { get; set; }
            public bool IsFolder { get; set; }
            public int Count { get; set; }
            public string cloud_file_id { get; set; }
            public string GoogleFileId { get; set; }

            private bool _isChecked;
            public bool IsChecked
            {
                get => _isChecked;
                set
                {
                    if (_isChecked != value)
                    {
                        _isChecked = value;
                        OnPropertyChanged(nameof(IsChecked));
                    }
                }
            }

            public string Icon => IsFolder ? "asset/folder.png" : "asset/file.png";

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string name)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        //////변환기
        private FileItemViewModel ToViewModel(CloudFileInfo file)
        {
            return new FileItemViewModel
            {
                FileId = file.FileId,
                FileName = file.FileName,
                FileSize = file.FileSize,
                UploadedAt = file.UploadedAt,
                CloudStorageNum = file.CloudStorageNum,
                ParentFolderId = file.ParentFolderId,
                IsFolder = file.IsFolder,
                cloud_file_id = file.CloudFileId,
                IsChecked = false
            };
        }


        /// 뷰 클래스에서 받은 정보를 DB에 저장하기 위한 변환기
        private CloudFileInfo ToCloudFileInfo(FileItemViewModel vm)
        {
            return new CloudFileInfo
            {
                FileId = vm.FileId,
                FileName = vm.FileName,
                FileSize = vm.FileSize,
                UploadedAt = vm.UploadedAt,
                CloudStorageNum = vm.CloudStorageNum,
                ParentFolderId = moveTargetFolderId, // 여기서만 목적지로 덮어씀
                IsFolder = vm.IsFolder,
                CloudFileId = vm.cloud_file_id

            };
        }


        ////////////////////////////////////////////////////////////////////////////////////////////////////////


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
                    bool result = await _fileUploadManager.file_upload(filePath, currentFolderId);

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

                        bool result = await UploadFolderRecursive(folderPath, currentFolderId);

                        System.Windows.MessageBox.Show(result
                            ? $"폴더 업로드 성공\n경로: {folderPath}"
                            : "폴더 업로드 실패");

                        LoadFolderContents(currentFolderId);
                        RefreshExplorer();
                    }
                }
            }
        }


        private async Task<bool> UploadFolderRecursive(string folderPath, int parentFolderId)
        {
            try
            {
                // 1. 현재 폴더를 DB에 등록
                var folderInfo = new CloudFileInfo
                {
                    FileName = Path.GetFileName(folderPath),
                    ParentFolderId = parentFolderId,
                    IsFolder = true,
                    UploadedAt = DateTime.Now,
                    FileSize = 0,
                    CloudStorageNum = -1,
                    CloudFileId = string.Empty,
                };

                int newFolderId = _fileRepository.add_folder(folderInfo);
                if (newFolderId == -1)
                {
                    System.Windows.MessageBox.Show($"폴더 '{folderInfo.FileName}' 등록 실패");
                    return false;
                }

                // 2. 현재 폴더 내 파일 업로드
                var files = Directory.GetFiles(folderPath);
                foreach (var filePath in files)
                {
                    bool uploadResult = await _fileUploadManager.file_upload(filePath, newFolderId);
                    if (!uploadResult)
                    {
                        System.Windows.MessageBox.Show($"파일 '{Path.GetFileName(filePath)}' 업로드 실패");
                    }
                }

                // 3. 하위 폴더 재귀 처리
                var subfolders = Directory.GetDirectories(folderPath);
                foreach (var subfolderPath in subfolders)
                {
                    await UploadFolderRecursive(subfolderPath, newFolderId);
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"오류 발생: {ex.Message}");
                return false;
            }
        }

        /// //////////////////////////////////////////////////////////////////////////////////

        //_fileRepository._fileRepository.all_file_list

        private void LoadRootFolders()
        {
            // "모든 파일" 루트 노드
            var rootItem = new TreeViewItem
            {
                Header = "Over cloud",
                Tag = -1
            };

            // 바로 하위 폴더만 조회해서 추가
            var rootChildren = _fileRepository.all_file_list(-1)
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

                    var children = _fileRepository.all_file_list(parentId)
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
            // 1) 현재 확장된 노드들의 ID를 수집
            var expandedIds = new HashSet<int>();
            CollectExpandedIds(FileExplorerTree.Items, expandedIds);

            // 2) 트리 클리어 & 루트 로드
            FileExplorerTree.Items.Clear();
            LoadRootFolders();

            // 3) 저장된 ID에 해당하는 노드 다시 펼치기
            RestoreExpandedState(FileExplorerTree.Items, expandedIds);
        }

        // 재귀적으로 TreeViewItem에서 IsExpanded된 Tag(int) 수집
        private void CollectExpandedIds(ItemCollection items, HashSet<int> ids)
        {
            foreach (var obj in items.OfType<TreeViewItem>())
            {
                int id = (int)obj.Tag;
                if (obj.IsExpanded) ids.Add(id);
                CollectExpandedIds(obj.Items, ids);
            }
        }

        // 재귀적으로 ID가 있으면 다시 확장 및 자식 로드
        private void RestoreExpandedState(ItemCollection items, HashSet<int> ids)
        {
            foreach (var tvi in items.OfType<TreeViewItem>())
            {
                int id = (int)tvi.Tag;
                if (ids.Contains(id))
                {
                    tvi.IsExpanded = true;

                    // “Loading…” 있으면 실제 하위 폴더로 교체
                    if (tvi.Items.Count == 1 && tvi.Items[0] is string s && s == "Loading...")
                    {
                        tvi.Items.Clear();
                        var children = _fileRepository.all_file_list(id).Where(f => f.IsFolder);
                        foreach (var f in children)
                        {
                            var childTvi = new TreeViewItem
                            {
                                Header = f.FileName,
                                Tag = f.FileId
                            };
                            childTvi.Items.Add("Loading...");
                            childTvi.Expanded += Folder_Expanded;
                            tvi.Items.Add(childTvi);
                        }
                    }

                    // 자식도 재귀 처리
                    RestoreExpandedState(tvi.Items, ids);
                }
            }
        }



        private void FileExplorerTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is TreeViewItem item && item.Tag is int folderId)
            {
                currentFolderId = folderId;
                LoadFolderContents(currentFolderId);
                if (isMoveMode)
                {
                    moveTargetFolderId = folderId;
                }
            }
            Console.WriteLine("현제 폴더 위치 변경 : " + currentFolderId);
        }

        private void LoadFolderContents(int folderId)
        {
            var contents = _fileRepository.all_file_list(folderId)
                .Select(file => ToViewModel(file))
                .ToList();

            RightFileListPanel.ItemsSource = contents;
            DateColumnPanel.ItemsSource = contents;
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
                    var folder = _fileRepository.all_file_list(currentFolderId)
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
                            var children = _fileRepository.all_file_list(childId)
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





        private List<FileItemViewModel> GetCheckedFiles()
        {
            if (RightFileListPanel.ItemsSource is IEnumerable<FileItemViewModel> items)
            {
                return items.Where(f => f.IsChecked).ToList();
            }
            return new List<FileItemViewModel>();
        }

        private async void Button_Down_Click(object sender, RoutedEventArgs e)
        {
            var selectedFiles = GetCheckedFiles();
            if (selectedFiles.Count == 0)
            {
                System.Windows.MessageBox.Show("선택된 항목이 없습니다.");
                return;
            }

            string localBase = "";
            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "파일을 저장할 폴더를 선택하세요.";
                if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    localBase = folderDialog.SelectedPath;
                else
                {
                    System.Windows.MessageBox.Show("저장할 폴더를 선택하지 않았습니다.");
                    return;
                }
            }

            var allMap = GetAllFilesFromCurrentFolder(); // 현제 디렉토리 하위의 모든 트리 정보 { fileId → 정보 }

            try
            {
                foreach (var item in selectedFiles)
                {
                    await DownloadItemRecursive(item.FileId, localBase, allMap);
                }

                System.Windows.MessageBox.Show("다운로드 완료");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"다운로드 중 오류 발생: {ex.Message}");
            }
        }

        private async Task DownloadItemRecursive(int fileId, string localBase, Dictionary<int, CloudFileInfo> current_file_map)
        {
            if (!current_file_map.TryGetValue(fileId, out var file)) return;

            string cloudPath = GetCloudPath(file, current_file_map);
            string localPath = Path.Combine(localBase, cloudPath);

            if (file.IsFolder)
            {
                Directory.CreateDirectory(localPath);

                var children = _fileRepository.all_file_list(file.FileId); // 이 폴더의 하위 항목
                foreach (var child in children)
                {
                    await DownloadItemRecursive(child.FileId, localBase, current_file_map);
                }
            }
            else
            {
                string? dir = Path.GetDirectoryName(localPath);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);

                Console.WriteLine(dir);
                Console.WriteLine(file.CloudFileId + " " + file.CloudStorageNum);

                await _fileDownloadManager.DownloadFile("admin", file.CloudFileId, file.CloudStorageNum, localPath);
            }
        }

        private string GetCloudPath(CloudFileInfo file, Dictionary<int, CloudFileInfo> allMap)
        {
            var parts = new List<string> { file.FileName };
            var current = file;
            while (current.ParentFolderId != null && allMap.TryGetValue(current.ParentFolderId, out var parent))
            {
                parts.Insert(0, parent.FileName);
                current = parent;
            }
            return Path.Combine(parts.ToArray());
        }

        private Dictionary<int, CloudFileInfo> GetAllFilesFromCurrentFolder()
        {
            var result = new Dictionary<int, CloudFileInfo>();

            void Traverse(int parentId)
            {
                var children = _fileRepository.all_file_list(parentId);
                foreach (var file in children)
                {
                    result[file.FileId] = file;
                    if (file.IsFolder)
                        Traverse(file.FileId);
                }
            }

            Traverse(currentFolderId); // 현재 보고 있는 폴더부터 시작
            return result;
        }


        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////삭제 버튼 클릭 시

        private async void Button_DeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            var checkedItems = GetCheckedFiles();
            if (checkedItems.Count == 0)
            {
                System.Windows.MessageBox.Show("선택된 항목이 없습니다.");
                return;
            }

            var confirm = System.Windows.MessageBox.Show(
                $"총 {checkedItems.Count}개의 항목을 삭제하시겠습니까?",
                "삭제 확인",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes)
                return;

            var allFileMap = GetAllFilesFromCurrentFolder(); // 현재 폴더 기준 전체 파일 맵

            foreach (var item in checkedItems)
            {
                await DeleteItemRecursive(item.FileId, allFileMap);
            }

            // UI 갱신
            LoadFolderContents(currentFolderId);
            RefreshExplorer();
        }


        private async Task DeleteItemRecursive(int fileId, Dictionary<int, CloudFileInfo> allFileMap)
        {
            if (!allFileMap.TryGetValue(fileId, out var file)) return;

            if (file.IsFolder)
            {
                var children = _fileRepository.all_file_list(file.FileId);
                foreach (var child in children)
                {
                    await DeleteItemRecursive(child.FileId, allFileMap);
                }
            }

            // 비동기 삭제 호출

            bool deleted = await _fileDeleteManager.Delete_File(file.CloudStorageNum, file.FileId);


            if (!deleted)
            {
                System.Windows.MessageBox.Show($"{file.FileName} 삭제 실패");
            }
        }


        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        ///


        //////////////////////////////////////////////////////////////////////////////////////////////////////
        ///이동 버튼 클릭 시
        private void Button_Move_Click(object sender, RoutedEventArgs e)
        {
            var selected = GetCheckedFiles();
            if (selected.Count == 0)
            {
                System.Windows.MessageBox.Show("이동할 항목을 선택하세요.");
                return;
            }

            var dialog = new FolderSelectDialog(_fileRepository)
            {
                Owner = Window.GetWindow(this)
            };

            if (dialog.ShowDialog() == true)
            {
                int targetFolderId = dialog.SelectedFolderId.Value;

                foreach (var item in selected)
                {
                    var cloudInfo = ToCloudFileInfo(item);
                    cloudInfo.ParentFolderId = targetFolderId;
                    _fileRepository.change_dir(cloudInfo);
                }

                LoadFolderContents(currentFolderId);
                RefreshExplorer();

                System.Windows.MessageBox.Show("이동이 완료되었습니다.");
            }
        }

        /*
        private void Button_ConfirmMove_Click(object sender, RoutedEventArgs e)
        {
            if (!isMoveMode || moveTargetFolderId == -2 || moveCandidates.Count == 0)
            {
                System.Windows.MessageBox.Show("이동할 항목 또는 대상 폴더가 지정되지 않았습니다.");
                return;
            }

            foreach (var item in moveCandidates)
            {
                var cloudInfo = ToCloudFileInfo(item);
                var result = _fileRepository.change_dir(cloudInfo);
            }

            isMoveMode = false;
            moveTargetFolderId = -2;
            moveCandidates.Clear();


            UploadButton.Visibility = Visibility.Visible;
            DownloadButton.Visibility = Visibility.Visible;
            DeleteButton.Visibility = Visibility.Visible;
            MoveButton.Visibility = Visibility.Visible;
            CopyButton.Visibility = Visibility.Visible;
            AddFolderButton.Visibility = Visibility.Visible;

            MoveModePanel.Visibility = Visibility.Collapsed;
            PageTitleTextBlock.Text = "홈";

            LoadFolderContents(currentFolderId);
            RefreshExplorer();

            System.Windows.MessageBox.Show("이동이 완료되었습니다.");
        }

        private void Button_CancelMove_Click(object sender, RoutedEventArgs e)
        {
            isMoveMode = false;
            moveTargetFolderId = -2;
            moveCandidates.Clear();

            UploadButton.Visibility = Visibility.Visible;
            DownloadButton.Visibility = Visibility.Visible;
            DeleteButton.Visibility = Visibility.Visible;
            MoveButton.Visibility = Visibility.Visible;
            CopyButton.Visibility = Visibility.Visible;
            AddFolderButton.Visibility = Visibility.Visible;

            MoveModePanel.Visibility = Visibility.Collapsed;
            PageTitleTextBlock.Text = "홈";

        }*/


        ///////////////////////////////////////////////////////////////////////////////////////////////////////
        ///폴더 추가
        private void Button_AddFolder_Click(object sender, RoutedEventArgs e)
        {
            // 다이얼로그 띄우기
            var dlg = new AddFolderDialog();
            // this가 아니라 이 UserControl을 포함하고 있는 Window를 Owner로 지정
            dlg.Owner = Window.GetWindow(this);
            if (dlg.ShowDialog() != true)
                return;

            // 입력한 이름으로 CloudFileInfo 생성
            var info = new CloudFileInfo
            {
                FileName = dlg.FolderName,
                ParentFolderId = currentFolderId,
                IsFolder = true,
                UploadedAt = DateTime.Now,
                FileSize = 0,
                CloudStorageNum = -1,
                CloudFileId = string.Empty,
            };

            // DB에 삽입
            int result;

            try
            {
                result = _fileRepository.add_folder(info);
                //result = null;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"폴더 추가 중 오류: {ex.Message}", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (result == -1)

            {
                System.Windows.MessageBox.Show("폴더 추가에 실패했습니다.", "오류", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // UI 갱신
            LoadFolderContents(currentFolderId);
            RefreshExplorer();


        }


        ////////////////////////////////////////////////////////////////////////////////////////
        ///복사 코드
        ///

        private async void Button_Copy_Click(object sender, RoutedEventArgs e)
        {
            var selected = GetCheckedFiles();
            if (selected.Count == 0)
            {
                System.Windows.MessageBox.Show("복사할 항목을 선택하세요.");
                return;
            }

            var dialog = new FolderSelectDialog(_fileRepository)
            {
                Owner = Window.GetWindow(this)
            };

            if (dialog.ShowDialog() == true)
            {
                int targetFolderId = dialog.SelectedFolderId.Value;

                foreach (var item in selected)
                {
                    bool result = await _fileCopyManager.Copy_File(item.FileId, targetFolderId);
                    if (!result)
                    {
                        System.Windows.MessageBox.Show($"파일/폴더 '{item.FileName}' 복사 실패");
                    }
                }

                LoadFolderContents(currentFolderId);
                RefreshExplorer();

                System.Windows.MessageBox.Show("복사가 완료되었습니다.");
            }
        }

        private async Task<bool> CopyFolderRecursive(int sourceFolderId, int targetParentFolderId)
        {
            var folderInfo = _fileRepository.specific_file_info(sourceFolderId);
            if (folderInfo == null || !folderInfo.IsFolder)
                return false;

            // 1. 현재 폴더를 targetParentFolderId 아래 새로 추가
            var newFolderInfo = new CloudFileInfo
            {
                FileName = folderInfo.FileName,
                ParentFolderId = targetParentFolderId,
                IsFolder = true,
                UploadedAt = DateTime.Now,
                FileSize = 0,
                CloudStorageNum = -1,
                CloudFileId = string.Empty
            };

            int newFolderId = _fileRepository.add_folder(newFolderInfo);
            if (newFolderId == -1)
            {
                System.Windows.MessageBox.Show($"폴더 '{newFolderInfo.FileName}' 복사 실패");
                return false;
            }

            // 2. 하위 항목 재귀 복사
            var children = _fileRepository.all_file_list(sourceFolderId);
            foreach (var child in children)
            {
                if (child.IsFolder)
                {
                    // 하위 폴더면 재귀 호출
                    await CopyFolderRecursive(child.FileId, newFolderId);
                }
                else
                {
                    // 파일이면 파일 복사 (Copy_File 호출)
                    await _fileCopyManager.Copy_File(child.FileId, newFolderId);
                }
            }

            return true;
        }



    }
}

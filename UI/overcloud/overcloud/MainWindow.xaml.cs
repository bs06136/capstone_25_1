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
using DB.overcloud.Service;

namespace overcloud
{
    public partial class MainWindow : Window
    {
        private AccountService _accountService;
        private FileUploadManager _FileUploadManager;
        private GoogleDriveService _GoogleDriveService;

        public MainWindow()
        {
            InitializeComponent();

            // 1) Repository 인스턴스(예: AccountRepository) 준비
            IAccountRepository repo = new AccountRepository(DbConfig.ConnectionString);
            IStorageService repo_2 = new StorageService(DbConfig.ConnectionString);

            // 2) AccountService에 주입
            _accountService = new AccountService(repo, repo_2);
            _GoogleDriveService = new GoogleDriveService();
            _FileUploadManager = new FileUploadManager(_accountService, _GoogleDriveService);
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
            List<string> directories = all_file_list(); // 또는 적절한 클래스에서 호출
            FileListGrid.ItemsSource = directories;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows.Forms;
using static overcloud.temp_class.TempClass;
using OverCloud.Services;
using DB.overcloud.Service;
using overcloud.Views;

namespace overcloud
{
    public partial class MainWindow : Window
    {
        private readonly IAccountRepository _accountRepo;
        private readonly IStorageService _storageService;

        public MainWindow()
        {
            InitializeComponent();

            _accountRepo = new AccountRepository(DbConfig.ConnectionString);
            _storageService = new StorageService(DbConfig.ConnectionString);
        }

        private void Button_Add_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddAccountWindow(_accountRepo);
            addWindow.Owner = this;
            addWindow.ShowDialog();

            RefreshAccountListAndPieChart();
        }

        private void Button_Delete_Click(object sender, RoutedEventArgs e)
        {
            var deleteWindow = new DeleteAccountWindow(_accountRepo);
            deleteWindow.Owner = this;
            deleteWindow.ShowDialog();

            RefreshAccountListAndPieChart();
        }

        private void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            var choice = System.Windows.MessageBox.Show(
                "파일을 선택하려면 [예], 폴더를 선택하려면 [아니오]를 클릭하세요.",
                "선택 방식",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            if (choice == MessageBoxResult.Yes)
            {
                var fileDialog = new CommonOpenFileDialog
                {
                    IsFolderPicker = false,
                    Multiselect = false,
                    Title = "파일 선택"
                };

                if (fileDialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    string filePath = fileDialog.FileName;
                    System.Windows.MessageBox.Show($"파일 선택됨: {filePath}");
                }
            }
            else if (choice == MessageBoxResult.No)
            {
                using var folderDialog = new FolderBrowserDialog
                {
                    Description = "폴더 선택",
                    RootFolder = Environment.SpecialFolder.MyComputer
                };

                if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string folderPath = folderDialog.SelectedPath;
                    System.Windows.MessageBox.Show($"폴더 선택됨: {folderPath}");
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

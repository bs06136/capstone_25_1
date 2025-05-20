using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DB.overcloud.Repository;
using System.Windows.Controls;
using System.Windows;

namespace overcloud.Views
{
    public partial class FolderSelectDialog : Window
    {
        private readonly IFileRepository _fileRepository;
        public int? SelectedFolderId { get; private set; } = null;

        public FolderSelectDialog(IFileRepository fileRepository)
        {
            InitializeComponent();
            _fileRepository = fileRepository;
            LoadFolders();
        }

        private void LoadFolders()
        {
            var rootItem = new TreeViewItem { Header = "Over cloud", Tag = -1 };
            LoadChildFolders(rootItem, -1);
            FolderTreeView.Items.Add(rootItem);
        }

        private void LoadChildFolders(TreeViewItem parentItem, int parentId)
        {
            var folders = _fileRepository.all_file_list(parentId)
                .Where(f => f.IsFolder)
                .ToList();

            foreach (var folder in folders)
            {
                var item = new TreeViewItem
                {
                    Header = folder.FileName,
                    Tag = folder.FileId
                };

                item.Expanded += (s, e) =>
                {
                    if (item.Items.Count == 0)
                        LoadChildFolders(item, (int)item.Tag);
                };

                parentItem.Items.Add(item);
            }
        }

        private void FolderTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is TreeViewItem item && item.Tag is int id)
            {
                SelectedFolderId = id;
            }
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedFolderId == null)
            {
                System.Windows.MessageBox.Show("폴더를 선택하세요.");
                return;
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }

}

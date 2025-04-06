using System.Collections.ObjectModel;
using System.Windows.Controls;
using DB.overcloud.Models;

namespace overcloud.Converters
{
    public class FileTreeNode : TreeViewItem
    {
        public CloudFileInfo FileInfo { get; private set; }
        public ObservableCollection<FileTreeNode> ChildrenNodes { get; private set; } = new();

        public FileTreeNode(CloudFileInfo fileInfo)
        {
            FileInfo = fileInfo;

            // 체크박스 + 이름으로 Header 구성
            var panel = new StackPanel { Orientation = System.Windows.Controls.Orientation.Horizontal };
            var checkbox = new System.Windows.Controls.CheckBox();
            checkbox.Margin = new System.Windows.Thickness(0, 0, 5, 0);
            checkbox.VerticalAlignment = System.Windows.VerticalAlignment.Center;

            var binding = new System.Windows.Data.Binding("IsChecked")
            {
                Source = this,
                Mode = System.Windows.Data.BindingMode.TwoWay
            };
            checkbox.SetBinding(System.Windows.Controls.CheckBox.IsCheckedProperty, binding);

            panel.Children.Add(checkbox);
            panel.Children.Add(new TextBlock { Text = fileInfo.FileName });

            Header = panel;

            ItemsSource = ChildrenNodes;

            if (fileInfo.IsFolder)
                ChildrenNodes.Add(new FileTreeNodeDummy());
        }

        public bool IsLoaded =>
            !(ChildrenNodes.Count == 1 && ChildrenNodes[0] is FileTreeNodeDummy);

        public void LoadChildren(List<CloudFileInfo> children)
        {
            ChildrenNodes.Clear();

            foreach (var child in children)
            {
                var node = new FileTreeNode(child);
                ChildrenNodes.Add(node);
            }
        }


        public bool IsChecked { get; set; } = false;


    }

    // 내부 dummy 노드 클래스
    public class FileTreeNodeDummy : FileTreeNode
    {
        public FileTreeNodeDummy() : base(new CloudFileInfo { FileName = "Loading...", IsFolder = false }) { }

    }
}

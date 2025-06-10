using System.Windows;
using SourceChord.FluentWPF;

namespace overcloud.Views
{
    public partial class AddFolderDialog : AcrylicWindow
    {
        public string FolderName => FolderNameTextBox.Text.Trim();

        public AddFolderDialog()
        {
            InitializeComponent();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FolderName))
            {
                System.Windows.MessageBox.Show("폴더 이름을 입력해주세요.", "경고", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            this.DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}

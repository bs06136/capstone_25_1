using System.Windows.Controls;
using overcloud.transfer_manager;

namespace overcloud.Views
{
    public partial class UploadTabView : System.Windows.Controls.UserControl
    {
        public UploadTabView()
        {
            InitializeComponent();
            DataContext = App.TransferManager.UploadManager;// App.TransferManager.UploadManager 를 바인딩함
        }
    }
}

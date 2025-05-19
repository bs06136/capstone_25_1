using System.Windows.Controls;
using overcloud.transfer_manager;

namespace overcloud.Views
{
    public partial class DownloadTabView : System.Windows.Controls.UserControl
    {
        public DownloadTabView()
        {
            InitializeComponent();
            DataContext = App.TransferManager.DownloadManager;// App.TransferManager.DownloadManager 를 바인딩함
        }
    }
}

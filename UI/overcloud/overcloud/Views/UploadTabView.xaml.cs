using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
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



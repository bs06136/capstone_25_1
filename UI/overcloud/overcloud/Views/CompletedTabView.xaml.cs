using System.Windows.Controls;

namespace overcloud.Views
{
    public partial class CompletedTabView : System.Windows.Controls.UserControl
    {
        public CompletedTabView()
        {
            InitializeComponent();
            DataContext = App.TransferManager; // App.TransferManager.Completed 을 바인딩함
        }
    }
}

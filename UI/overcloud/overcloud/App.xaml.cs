using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Windows;
using OverCloud.transfer_manager;

namespace overcloud
{
    public partial class App : System.Windows.Application
    {
        public static TransferManager TransferManager { get;  set; }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var loginWindow = new overcloud.Views.LoginWindow();
            loginWindow.Show();
        }
    }
}

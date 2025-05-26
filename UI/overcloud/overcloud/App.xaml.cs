using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.Windows;
using OverCloud.Services;
using OverCloud.transfer_manager;

namespace overcloud
{
    public partial class App : System.Windows.Application
    {
        private LoginController _controller;
        public static TransferManager TransferManager { get;  set; }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _controller = new LoginController();

            var loginWindow = new overcloud.Views.LoginWindow(_controller);
            loginWindow.Show();
        }
    }
}

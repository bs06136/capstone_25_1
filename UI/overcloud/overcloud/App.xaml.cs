using System.Configuration;
using System.Data;
using System.Windows;

namespace overcloud
{
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var loginWindow = new overcloud.Views.LoginWindow();
            loginWindow.Show();
        }
    }
}

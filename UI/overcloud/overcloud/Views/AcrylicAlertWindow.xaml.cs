using SourceChord.FluentWPF;
using System.Windows;

namespace overcloud.Views
{
    public partial class AcrylicAlertWindow : AcrylicWindow
    {
        public AcrylicAlertWindow(string message)
        {
            InitializeComponent();
            MessageTextBox.Text = message;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

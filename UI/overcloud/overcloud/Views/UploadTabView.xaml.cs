using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Animation;
using OverCloud.transfer_manager;

namespace overcloud.Views
{
    public partial class UploadTabView : System.Windows.Controls.UserControl
    {
        public UploadTabView()
        {
            InitializeComponent();
            DataContext = App.TransferManager.UploadManager;// App.TransferManager.UploadManager 를 바인딩함
        }

        private void ProgressBar_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.ProgressBar pb && pb.DataContext is TransferItemViewModel vm)
            {
                if (vm.Progress < 100)
                {
                    var animation = new DoubleAnimation
                    {
                        From = pb.Value,
                        To = vm.Progress,
                        Duration = TimeSpan.FromMilliseconds(300),
                        EasingFunction = new QuadraticEase() { EasingMode = EasingMode.EaseOut }
                    };
                    pb.BeginAnimation(System.Windows.Controls.ProgressBar.ValueProperty, animation);
                }
                else
                {
                    pb.Value = 100; // 즉시 완료
                }
            }
        }


    }
}



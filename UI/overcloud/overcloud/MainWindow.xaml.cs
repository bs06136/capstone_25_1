using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using overcloud.Views;
using overcloud.Models;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows.Forms;
using OverCloud.Services;
using DB.overcloud.Service;

namespace overcloud
{
    public partial class MainWindow : Window
    {
        private readonly AccountService _accountService;

        public MainWindow()
        {
            InitializeComponent();

            var repo = new AccountRepository(DbConfig.ConnectionString);
            _accountService = new AccountService(repo);
        }

        private void Button_Add_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddAccountWindow(_accountService);
            addWindow.ShowDialog();
        }

        private void Button_Delete_Click(object sender, RoutedEventArgs e)
        {
            var deleteWindow = new DeleteAccountWindow(_accountService);
            deleteWindow.Owner = this;
            deleteWindow.ShowDialog();
        }

        private void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            var choice = MessageBox.Show(
                "파일을 선택하려면 [예], 폴더를 선택하려면 [아니오]를 클릭하세요.",
                "선택 방식",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            if (choice == MessageBoxResult.Yes)
            {
                var fileDialog = new CommonOpenFileDialog()
                {
                    IsFolderPicker = false,
                    Multiselect = false,
                    Title = "파일 선택"
                };

                if (fileDialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    string filePath = fileDialog.FileName;
                    bool result = true;

                    MessageBox.Show(result
                        ? $"파일 업로드 성공\n경로: {filePath}"
                        : "파일 업로드 실패");
                }
            }
            else if (choice == MessageBoxResult.No)
            {
                using (var folderDialog = new FolderBrowserDialog())
                {
                    folderDialog.Description = "폴더 선택";
                    folderDialog.RootFolder = System.Environment.SpecialFolder.MyComputer;

                    if (folderDialog.ShowDialog() == DialogResult.OK)
                    {
                        string folderPath = folderDialog.SelectedPath;
                        bool result = true;

                        MessageBox.Show(result
                            ? $"폴더 업로드 성공\n경로: {folderPath}"
                            : "폴더 업로드 실패");
                    }
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DrawDetailedPieChart();

            List<CloudAccountInfo> accountList = _accountService.GetAllAccounts();
            CloudListGrid.ItemsSource = accountList;
        }

        private void DrawDetailedPieChart()
        {
            Canvas.SetLeft(PieCanvas, 0);
            Canvas.SetTop(PieCanvas, 0);

            List<CloudAccountInfo> cloudList = _accountService.GetAllAccounts();

            PieCanvas.Children.Clear();
            double radius = 130;
            Point center = new Point(radius + 20, radius + 20);

            double totalSize = 0;
            foreach (var cloud in cloudList)
                totalSize += cloud.TotalSize;

            double startAngle = 0;
            Color[] baseColors = { Colors.DodgerBlue, Colors.Orange, Colors.ForestGreen };
            int colorIndex = 0;

            foreach (var cloud in cloudList)
            {
                double usedAngle = (cloud.UsedSize / totalSize) * 360;
                Path usedSlice = CreatePieSlice(center, radius, startAngle, usedAngle, baseColors[colorIndex]);
                PieCanvas.Children.Add(usedSlice);

                AddLabel(center, radius, startAngle, usedAngle,
                    $"{cloud.CloudType}\nUsed:{cloud.UsedSize / 1e6:F1}MB", Brushes.White);

                startAngle += usedAngle;

                double remainingSize = cloud.TotalSize - cloud.UsedSize;
                double remainingAngle = (remainingSize / totalSize) * 360;
                Color lighterColor = ChangeColorBrightness(baseColors[colorIndex], 0.5f);
                Path remainingSlice = CreatePieSlice(center, radius, startAngle, remainingAngle, lighterColor);
                PieCanvas.Children.Add(remainingSlice);

                AddLabel(center, radius, startAngle, remainingAngle,
                    $"{cloud.CloudType}\nFree:{remainingSize / 1e6:F1}MB", Brushes.Black);

                startAngle += remainingAngle;
                colorIndex++;
            }
        }

        private void AddLabel(Point center, double radius, double startAngle, double angle, string text, Brush color)
        {
            double midAngle = startAngle + angle / 2;
            double radian = midAngle * Math.PI / 180;
            double labelRadius = radius * 0.7;

            var label = new TextBlock
            {
                Text = text,
                Foreground = color,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center
            };

            Canvas.SetLeft(label, center.X + labelRadius * Math.Cos(radian) - 40);
            Canvas.SetTop(label, center.Y + labelRadius * Math.Sin(radian) - 20);

            PieCanvas.Children.Add(label);
        }

        private Path CreatePieSlice(Point center, double radius, double startAngle, double angle, Color color)
        {
            double radStart = startAngle * Math.PI / 180;
            double radEnd = (startAngle + angle) * Math.PI / 180;

            Point startPoint = new Point(center.X + radius * Math.Cos(radStart),
                                         center.Y + radius * Math.Sin(radStart));

            Point endPoint = new Point(center.X + radius * Math.Cos(radEnd),
                                       center.Y + radius * Math.Sin(radEnd));

            PathFigure figure = new PathFigure { StartPoint = center };
            figure.Segments.Add(new LineSegment(startPoint, true));
            figure.Segments.Add(new ArcSegment(endPoint, new Size(radius, radius), 0,
                angle > 180, SweepDirection.Clockwise, true));
            figure.Segments.Add(new LineSegment(center, true));

            PathGeometry geometry = new PathGeometry();
            geometry.Figures.Add(figure);

            return new Path
            {
                Fill = new SolidColorBrush(color),
                Stroke = Brushes.White,
                StrokeThickness = 2,
                Data = geometry
            };
        }

        public Color ChangeColorBrightness(Color color, float factor)
        {
            return Color.FromArgb(color.A,
                                  (byte)Math.Min(color.R + 255 * factor, 255),
                                  (byte)Math.Min(color.G + 255 * factor, 255),
                                  (byte)Math.Min(color.B + 255 * factor, 255));
        }
    }
}

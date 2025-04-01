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
//using static overcloud.temp_class.TempClass;
using OverCloud.Services;
using DB.overcloud.Service;

namespace overcloud
{
    public partial class MainWindow : Window
    {
        private AccountService _accountService;
        private GoogleDriveService _googleDriveService;
        private AccountManager _accountManager;

        public MainWindow()
        {
            InitializeComponent();

            // DB 연결용 Repository 생성
            string connStr = "server=localhost;database=overcloud;uid=admin;pwd=admin;";
            IAccountRepository repo = new AccountRepository(connStr);

            // 서비스들 주입
            _accountService = new AccountService(repo);
            _googleDriveService = new GoogleDriveService();
            _accountManager = new AccountManager(repo, _googleDriveService);


        }
        private void Button_Add_Click(object sender, RoutedEventArgs e)
        {
            AddAccountWindow window = new AddAccountWindow();
            window.ShowDialog();
        }

        private void Button_Delete_Click(object sender, RoutedEventArgs e)
        {
            DeleteAccountWindow window = new DeleteAccountWindow();
            window.Owner = this;
            window.ShowDialog();
        }


        private void Button_Save_Click(object sender, RoutedEventArgs e)
        {
            var choice = System.Windows.MessageBox.Show(
                "파일을 선택하려면 [예], 폴더를 선택하려면 [아니오]를 클릭하세요.",
                "선택 방식",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            if (choice == MessageBoxResult.Yes)
            {
                // 파일 선택
                var fileDialog = new CommonOpenFileDialog()
                {
                    IsFolderPicker = false,
                    Multiselect = false,
                    Title = "파일 선택"
                };

                if (fileDialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    string filePath = fileDialog.FileName;

                    // ⭐ temp_class.file_upload 호출
                    bool result = true;     //file_upload(filePath);

                    System.Windows.MessageBox.Show(result
                        ? $"파일 업로드 성공\n경로: {filePath}"
                        : "파일 업로드 실패");
                }
            }
            else if (choice == MessageBoxResult.No)
            {
                // 폴더 선택
                using (var folderDialog = new FolderBrowserDialog())
                {
                    folderDialog.Description = "폴더 선택";
                    folderDialog.RootFolder = System.Environment.SpecialFolder.MyComputer;

                    if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        string folderPath = folderDialog.SelectedPath;

                        // ⭐ temp_class.file_upload 호출
                        bool result = true;      //file_upload(folderPath);

                        System.Windows.MessageBox.Show(result
                            ? $"폴더 업로드 성공\n경로: {folderPath}"
                            : "폴더 업로드 실패");
                    }
                }
            }
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // 1) 원형 그래프 그리기 (사용 중인 로직)
            DrawDetailedPieChart();

            // 2) 오른쪽 DataGrid에 리스트 바인딩
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
            System.Windows.Point center = new System.Windows.Point(radius + 20, radius + 20);

            double totalSize = 0;
            foreach (var cloud in cloudList)
                totalSize += cloud.TotalSize;

            double startAngle = 0;

            // 각 클라우드별 색상 미리 설정
            System.Windows.Media.Color[] baseColors = { Colors.DodgerBlue, Colors.Orange, Colors.ForestGreen };
            int colorIndex = 0;

            foreach (var cloud in cloudList)
            {
                // 사용한 용량 슬라이스 그리기
                double usedAngle = (cloud.UsedSize / totalSize) * 360;
                Path usedSlice = CreatePieSlice(center, radius, startAngle, usedAngle, baseColors[colorIndex]);
                PieCanvas.Children.Add(usedSlice);

                AddLabel(center, radius, startAngle, usedAngle,
                    $"{cloud.CloudType}\nUsed:{cloud.UsedSize / 1e6:F1}MB", System.Windows.Media.Brushes.White);

                startAngle += usedAngle;

                // 남은 용량 슬라이스 그리기 (더 밝은 색상으로)
                double remainingSize = cloud.TotalSize - cloud.UsedSize;
                double remainingAngle = (remainingSize / totalSize) * 360;
                System.Windows.Media.Color lighterColor = ChangeColorBrightness(baseColors[colorIndex], 0.5f);
                Path remainingSlice = CreatePieSlice(center, radius, startAngle, remainingAngle, lighterColor);
                PieCanvas.Children.Add(remainingSlice);

                AddLabel(center, radius, startAngle, remainingAngle,
                    $"{cloud.CloudType}\nFree:{remainingSize / 1e6:F1}MB", System.Windows.Media.Brushes.Black);

                startAngle += remainingAngle;

                colorIndex++;
            }

        }

        private void AddLabel(System.Windows.Point center, double radius, double startAngle, double angle, string text, System.Windows.Media.Brush color)
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

        private Path CreatePieSlice(System.Windows.Point center, double radius, double startAngle, double angle, System.Windows.Media.Color color)
        {
            double radStart = startAngle * Math.PI / 180;
            double radEnd = (startAngle + angle) * Math.PI / 180;

            System.Windows.Point startPoint = new System.Windows.Point(center.X + radius * Math.Cos(radStart),
                                         center.Y + radius * Math.Sin(radStart));

            System.Windows.Point endPoint = new System.Windows.Point(center.X + radius * Math.Cos(radEnd),
                                       center.Y + radius * Math.Sin(radEnd));

            PathFigure figure = new PathFigure { StartPoint = center };
            figure.Segments.Add(new LineSegment(startPoint, true));
            figure.Segments.Add(new ArcSegment(endPoint, new System.Windows.Size(radius, radius), 0,
                angle > 180, SweepDirection.Clockwise, true));
            figure.Segments.Add(new LineSegment(center, true));

            PathGeometry geometry = new PathGeometry();
            geometry.Figures.Add(figure);

            return new Path
            {
                Fill = new SolidColorBrush(color),
                Stroke = System.Windows.Media.Brushes.White,
                StrokeThickness = 2,
                Data = geometry
            };
        }

        // 색상 밝기 조절 함수
        public System.Windows.Media.Color ChangeColorBrightness(System.Windows.Media.Color color, float factor)
        {
            return System.Windows.Media.Color.FromArgb(color.A,
                                  (byte)Math.Min(color.R + 255 * factor, 255),
                                  (byte)Math.Min(color.G + 255 * factor, 255),
                                  (byte)Math.Min(color.B + 255 * factor, 255));
        }
    }
}
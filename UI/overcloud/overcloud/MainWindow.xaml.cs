using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Windows.Forms;

using DB.overcloud.Models;
using DB.overcloud.Service;
using overcloud.Views;

namespace overcloud
{
    public partial class MainWindow : Window
    {
        private readonly IAccountRepository _accountRepo;
        private readonly IStorageService _storageService;

        public MainWindow()
        {
            InitializeComponent();

            _accountRepo = new AccountRepository(DbConfig.ConnectionString);
            _storageService = new StorageService(DbConfig.ConnectionString);
        }

        private void Button_Add_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddAccountWindow(_accountRepo);
            addWindow.Owner = this;
            addWindow.ShowDialog();

            RefreshAccountListAndPieChart();
        }

        private void Button_Delete_Click(object sender, RoutedEventArgs e)
        {
            var deleteWindow = new DeleteAccountWindow(_accountRepo);
            deleteWindow.Owner = this;
            deleteWindow.ShowDialog();

            RefreshAccountListAndPieChart();
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
                var fileDialog = new CommonOpenFileDialog
                {
                    IsFolderPicker = false,
                    Multiselect = false,
                    Title = "파일 선택"
                };

                if (fileDialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    string filePath = fileDialog.FileName;
                    System.Windows.MessageBox.Show($"파일 선택됨: {filePath}");
                }
            }
            else if (choice == MessageBoxResult.No)
            {
                using var folderDialog = new FolderBrowserDialog
                {
                    Description = "폴더 선택",
                    RootFolder = Environment.SpecialFolder.MyComputer
                };

                if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string folderPath = folderDialog.SelectedPath;
                    System.Windows.MessageBox.Show($"폴더 선택됨: {folderPath}");
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshAccountListAndPieChart();
        }

        private void RefreshAccountListAndPieChart()
        {
            var accounts = _accountRepo.GetAllAccounts();
            CloudListGrid.ItemsSource = accounts;
            DrawPieChart(accounts);
        }

        private void DrawPieChart(List<CloudAccountInfo> accounts)
        {
            PieCanvas.Children.Clear();
            double radius = 130;
            Point center = new Point(radius + 20, radius + 20);

            double totalSize = 0;
            foreach (var acc in accounts)
                totalSize += acc.TotalSize;

            if (totalSize == 0) return;

            double startAngle = 0;
            Color[] colors = { Colors.DodgerBlue, Colors.Orange, Colors.ForestGreen };
            int colorIndex = 0;

            foreach (var acc in accounts)
            {
                double usedAngle = (acc.UsedSize / totalSize) * 360;
                var usedSlice = CreatePieSlice(center, radius, startAngle, usedAngle, colors[colorIndex]);
                PieCanvas.Children.Add(usedSlice);

                AddLabel(center, radius, startAngle, usedAngle,
                    $"{acc.Username}\nUsed: {acc.UsedSize / 1e6:F1}MB", Brushes.White);
                startAngle += usedAngle;

                double remainingSize = acc.TotalSize - acc.UsedSize;
                double remainingAngle = (remainingSize / totalSize) * 360;
                var lighterColor = ChangeColorBrightness(colors[colorIndex], 0.5f);
                var remainingSlice = CreatePieSlice(center, radius, startAngle, remainingAngle, lighterColor);
                PieCanvas.Children.Add(remainingSlice);

                AddLabel(center, radius, startAngle, remainingAngle,
                    $"Free: {remainingSize / 1e6:F1}MB", Brushes.Black);
                startAngle += remainingAngle;

                colorIndex = (colorIndex + 1) % colors.Length;
            }
        }

        private Path CreatePieSlice(Point center, double radius, double startAngle, double angle, Color color)
        {
            double radStart = startAngle * Math.PI / 180;
            double radEnd = (startAngle + angle) * Math.PI / 180;

            Point startPoint = new Point(center.X + radius * Math.Cos(radStart),
                                         center.Y + radius * Math.Sin(radStart));

            Point endPoint = new Point(center.X + radius * Math.Cos(radEnd),
                                       center.Y + radius * Math.Sin(radEnd));

            var figure = new PathFigure { StartPoint = center };
            figure.Segments.Add(new LineSegment(startPoint, true));
            figure.Segments.Add(new ArcSegment(endPoint, new Size(radius, radius), 0,
                angle > 180, SweepDirection.Clockwise, true));
            figure.Segments.Add(new LineSegment(center, true));

            return new Path
            {
                Fill = new SolidColorBrush(color),
                Stroke = Brushes.White,
                StrokeThickness = 2,
                Data = new PathGeometry(new[] { figure })
            };
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

        public Color ChangeColorBrightness(Color color, float factor)
        {
            return Color.FromArgb(color.A,
                (byte)Math.Min(color.R + 255 * factor, 255),
                (byte)Math.Min(color.G + 255 * factor, 255),
                (byte)Math.Min(color.B + 255 * factor, 255));
        }
    }
}

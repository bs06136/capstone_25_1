using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using OverCloud.Services;
using DB.overcloud.Models;

namespace overcloud.Views
{
    public partial class DiskDetailWindow : Window
    {
        private AccountService _accountService;
        private bool isPieChartVisible = true;
        private string _user_id;

        private SolidColorBrush[] baseBrushes = {
            new SolidColorBrush(Colors.DodgerBlue),
            new SolidColorBrush(Colors.Orange),
            new SolidColorBrush(Colors.ForestGreen)
        };


        public class CloudDisplayModel
        {
            public string CloudType { get; set; }
            public string CloudId { get; set; }
            public double UsedSize { get; set; }
            public double TotalSize { get; set; }
            public System.Windows.Media.Brush ColorBrush { get; set; } // For Free (lighter)
            public System.Windows.Media.Brush UsedBrush { get; set; }   // For Used (darker)
        }

        public DiskDetailWindow(AccountService accountService, string user_id)
        {
            InitializeComponent();
            _accountService = accountService;
            Loaded += DiskDetailWindow_Loaded;
            _user_id = user_id;
        }

        private void DiskDetailWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var accountList = _accountService.Get_Clouds_For_User(_user_id);

            var displayList = accountList.Select((acc, idx) =>
            {
                var baseColor = baseBrushes[idx % baseBrushes.Length].Color;
                return new CloudDisplayModel
                {
                    CloudType = acc.CloudType,
                    CloudId = acc.AccountId,
                    UsedSize = acc.UsedCapacity,
                    TotalSize = acc.TotalCapacity,
                    ColorBrush = new SolidColorBrush(ChangeColorBrightness(baseColor, 0.5f)), // Used (그래프용)
                    UsedBrush = new SolidColorBrush(baseColor) // Free (표시용)
                };
            }).ToList();

            CloudListGrid.ItemsSource = displayList;

            DrawPieChart(displayList);
            DrawBarChart(displayList);
        }

        private void ToggleChartButton_Click(object sender, RoutedEventArgs e)
        {
            isPieChartVisible = !isPieChartVisible;

            PieCanvas.Visibility = isPieChartVisible ? Visibility.Visible : Visibility.Collapsed;
            BarCanvas.Visibility = isPieChartVisible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void AddLabelWithLine(Canvas canvas, System.Windows.Point center, double radius, double startAngle, double angle, string text, System.Windows.Media.Brush color)
        {
            double midAngle = startAngle + angle / 2;
            double radian = midAngle * Math.PI / 180;

            double innerRadius = radius;        // 선 시작점
            double outerRadius = radius + 20;   // 선 끝점
            double textRadius = radius + 40;    // 텍스트 위치

            // 선의 시작점과 끝점 계산
            var start = new System.Windows.Point(
                center.X + innerRadius * Math.Cos(radian),
                center.Y + innerRadius * Math.Sin(radian)
            );

            var end = new System.Windows.Point(
                center.X + outerRadius * Math.Cos(radian),
                center.Y + outerRadius * Math.Sin(radian)
            );

            var label = new TextBlock
            {
                Text = text,
                Foreground = color,
                FontWeight = FontWeights.Bold,
                FontSize = 12,
                TextAlignment = TextAlignment.Center
            };

            var line = new System.Windows.Shapes.Line
            {
                X1 = start.X,
                Y1 = start.Y,
                X2 = end.X,
                Y2 = end.Y,
                Stroke = color,
                StrokeThickness = 1
            };

            Canvas.SetLeft(label, center.X + textRadius * Math.Cos(radian) - 30);
            Canvas.SetTop(label, center.Y + textRadius * Math.Sin(radian) - 10);

            canvas.Children.Add(line);
            canvas.Children.Add(label);
        }

        private void DrawPieChart(List<CloudDisplayModel> cloudList)
        {
            PieCanvas.Children.Clear();
            Canvas.SetLeft(PieCanvas, 0);
            Canvas.SetTop(PieCanvas, 0);

            double radius = 130;
            System.Windows.Point center = new System.Windows.Point(radius + 20, radius + 20);
            double totalSize = cloudList.Sum(c => c.TotalSize);
            double startAngle = 0;

            foreach (var cloud in cloudList)
            {
                double usedAngle = (cloud.UsedSize / totalSize) * 360;
                System.Windows.Shapes.Path usedSlice = CreatePieSlice(center, radius, startAngle, usedAngle, ((System.Windows.Media.SolidColorBrush)cloud.UsedBrush).Color);
                PieCanvas.Children.Add(usedSlice);

                //AddLabel(PieCanvas, center, radius, startAngle, usedAngle,
                //    $"{cloud.CloudType}\nUsed:{cloud.UsedSize / 1e6:F1}MB", System.Windows.Media.Brushes.White);
                AddLabelWithLine(PieCanvas, center, radius, startAngle, usedAngle, $"{cloud.CloudId}\n/{cloud.CloudType}\nUsed:{cloud.UsedSize / 1000:F1}GB", System.Windows.Media.Brushes.Black);

                startAngle += usedAngle;

                double remainingSize = cloud.TotalSize - cloud.UsedSize;
                double remainingAngle = (remainingSize / totalSize) * 360;
                System.Windows.Media.Color lighterColor = ((System.Windows.Media.SolidColorBrush)cloud.ColorBrush).Color;
                System.Windows.Shapes.Path remainingSlice = CreatePieSlice(center, radius, startAngle, remainingAngle, lighterColor);
                PieCanvas.Children.Add(remainingSlice);

                //AddLabel(PieCanvas, center, radius, startAngle, remainingAngle,
                //    $"{cloud.CloudType}\nFree:{remainingSize / 1e6:F1}MB", System.Windows.Media.Brushes.Black);

                AddLabelWithLine(PieCanvas, center, radius, startAngle, remainingAngle, $"{cloud.CloudId}\n/{cloud.CloudType}\nFree:{remainingSize / 1000:F1}GB", System.Windows.Media.Brushes.Black);
                startAngle += remainingAngle;
            }
        }

        private void DrawBarChart(List<CloudDisplayModel> cloudList)
        {
            BarCanvas.Children.Clear();
            double barWidth = 40;
            double spacing = 40;
            double maxHeight = 200;
            double labelOffset = 10;
            double maxTotal = cloudList.Max(d => d.TotalSize);

            // Y축 텍스트 (예: 0, 50%, 100%)
            for (int i = 0; i <= 5; i++)
            {
                double value = maxTotal * i / 5;
                double y = maxHeight - (value / maxTotal) * maxHeight;
                var text = new TextBlock
                {
                    Text = $"{value / 1000}GB",
                    FontSize = 10,
                    Foreground = System.Windows.Media.Brushes.Black
                };
                Canvas.SetTop(text, y - 8);
                Canvas.SetLeft(text, 0);
                BarCanvas.Children.Add(text);
            }

            for (int i = 0; i < cloudList.Count; i++)
            {
                var cloud = cloudList[i];
                double x = 60 + i * (barWidth + spacing); // 시작 x 위치

                double usedHeight = (cloud.UsedSize / maxTotal) * maxHeight;
                double freeHeight = ((cloud.TotalSize - cloud.UsedSize) / maxTotal) * maxHeight;

                // Free (아래)
                var freeRect = new System.Windows.Shapes.Rectangle
                {
                    Width = barWidth,
                    Height = freeHeight,
                    Fill = cloud.ColorBrush
                };
                Canvas.SetLeft(freeRect, x);
                Canvas.SetTop(freeRect, maxHeight - freeHeight);
                BarCanvas.Children.Add(freeRect);

                // Used (위)
                var usedRect = new System.Windows.Shapes.Rectangle
                {
                    Width = barWidth,
                    Height = usedHeight,
                    Fill = cloud.UsedBrush
                };
                Canvas.SetLeft(usedRect, x);
                Canvas.SetTop(usedRect, maxHeight - freeHeight - usedHeight);
                BarCanvas.Children.Add(usedRect);

                // ID 라벨 (아래)
                var idLabel = new TextBlock
                {
                    Text = $"{cloud.CloudId}\n/{cloud.CloudType}",
                    FontSize = 10,
                    TextAlignment = TextAlignment.Center
                };
                Canvas.SetLeft(idLabel, x - 10);
                Canvas.SetTop(idLabel, maxHeight + labelOffset);
                BarCanvas.Children.Add(idLabel);
            }
        }


        private void AddLabel(Canvas canvas, System.Windows.Point center, double radius, double startAngle, double angle, string text, System.Windows.Media.Brush color)
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

            canvas.Children.Add(label);
        }

        private System.Windows.Shapes.Path CreatePieSlice(System.Windows.Point center, double radius, double startAngle, double angle, System.Windows.Media.Color color)
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

        private System.Windows.Media.Color ChangeColorBrightness(System.Windows.Media.Color color, float factor)
        {
            return System.Windows.Media.Color.FromArgb(color.A,
                (byte)Math.Min(color.R + 255 * factor, 255),
                (byte)Math.Min(color.G + 255 * factor, 255),
                (byte)Math.Min(color.B + 255 * factor, 255));
        }
    }
}

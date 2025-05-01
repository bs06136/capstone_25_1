using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Wpf;
using OverCloud.Services;
using OverCloud.Services.FileManager.DriveManager;
using OverCloud.Services.StorageManager;
using DB.overcloud.Repository;
using DB.overcloud.Models;
using Separator = LiveCharts.Wpf.Separator;

namespace overcloud.Views
{
    public partial class AccountDetailView : System.Windows.Controls.UserControl
    {
        private readonly AccountService _accountService;
        private readonly QuotaManager _quotaManager;

        // true = 막대 차트, false = 파이 차트
        private bool _isBarMode = true;
        private string _currentFilter = "All";

        public AccountDetailView()
        {
            InitializeComponent();

            // 서비스 초기화
            var connStr = DbConfig.ConnectionString;
            var storageRepo = new StorageRepository(connStr);
            var accountRepo = new AccountRepository(connStr);
            var tokenFactory = new TokenProviderFactory();
            var googleSvc = new GoogleDriveService(tokenFactory.CreateGoogleTokenProvider(), storageRepo, accountRepo);
            var oneDriveSvc = new OneDriveService(tokenFactory.CreateOneDriveTokenRefresher(), storageRepo, accountRepo);
            var cloudSvcs = new List<ICloudFileService> { googleSvc, oneDriveSvc };

            _quotaManager = new QuotaManager(cloudSvcs, storageRepo, accountRepo);
            _accountService = new AccountService(accountRepo, storageRepo, _quotaManager);

            // 초기 탭 선택
            FilterTab.SelectedIndex = 0;
            // 초기 토글 버튼 텍스트
            ToggleChartButton.Content = "파이 차트";
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // 첫 로드 시
            LoadUsageDetails("All");
            LoadChart("All");
        }

        private void FilterTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FilterTab.SelectedItem is TabItem ti)
            {
                _currentFilter = ti.Header.ToString();
                LoadUsageDetails(_currentFilter);
                LoadChart(_currentFilter);
            }
        }

        private void LoadUsageDetails(string filter)
        {
            // 1) 전체 계정 정보 가져오기
            var all = _accountService.Get_Clouds_For_User("user123");

            // 2) 필터링
            var list = filter == "All"
                ? all
                : all.Where(a => a.CloudType.Equals(filter, StringComparison.OrdinalIgnoreCase)).ToList();

            // 3) 뷰모델 생성
            var items = new List<UsageItemViewModel>();
            int total = 0, used = 0;
            foreach (var a in list)
            {
                total += a.TotalCapacity;
                used += a.UsedCapacity;

                items.Add(new UsageItemViewModel
                {
                    DriveName = a.CloudType,
                    TotalDisplay = $"{a.TotalCapacity}GB",
                    UsedDisplay = $"{a.UsedCapacity}GB",
                    UsedPercent = (int)(a.UsedCapacity * 100.0 / a.TotalCapacity)
                });
            }

            // 합계 추가
            items.Insert(0, new UsageItemViewModel
            {
                DriveName = "Total",
                TotalDisplay = $"{total}GB",
                UsedDisplay = $"{used}GB",
                UsedPercent = total > 0 ? (int)(used * 100.0 / total) : 0
            });

            UsageList.ItemsSource = items;
        }

        private void LoadChart(string filter)
        {
            var all = _accountService.Get_Clouds_For_User("user123");

            if (_isBarMode)
            {
                // 막대 그래프
                var cart = new CartesianChart
                {
                    Series = new SeriesCollection
                    {
                        new ColumnSeries
                        {
                            Title  = "Used",
                            Values = new ChartValues<double>(all.Select(a => (double)a.UsedCapacity))
                        },
                        new ColumnSeries
                        {
                            Title  = "Free",
                            Values = new ChartValues<double>(all.Select(a => (double)(a.TotalCapacity - a.UsedCapacity)))
                        }
                    }
                };
                cart.AxisX.Add(new Axis
                {
                    Labels = all.Select(a => a.CloudType).ToArray(),
                    Separator = new Separator { Step = 1 }
                });
                cart.AxisY.Add(new Axis { Title = "Storage (GB)" });

                ChartContainer.Content = cart;
            }
            else
            {
                // 파이 그래프
                // All 탭: 각 클라우드별 Used/Free 파이 조각
                // 개별 탭: 해당 클라우드만
                var list = filter == "All"
                    ? all
                    : all.Where(a => a.CloudType.Equals(filter, StringComparison.OrdinalIgnoreCase)).ToList();

                if (!list.Any())
                {
                    ChartContainer.Content = null;
                    return;
                }

                var pie = new PieChart
                {
                    LegendLocation = LegendLocation.Right,
                    Series = new SeriesCollection()
                };

                foreach (var acc in list)
                {
                    pie.Series.Add(new PieSeries
                    {
                        Title = $"{acc.CloudType} Used ({acc.UsedCapacity}GB)",
                        Values = new ChartValues<double> { acc.UsedCapacity },
                        DataLabels = true
                    });
                    pie.Series.Add(new PieSeries
                    {
                        Title = $"{acc.CloudType} Free ({acc.TotalCapacity - acc.UsedCapacity}GB)",
                        Values = new ChartValues<double> { acc.TotalCapacity - acc.UsedCapacity },
                        DataLabels = true
                    });
                }

                ChartContainer.Content = pie;
            }
        }

        private void ToggleChart_Click(object sender, RoutedEventArgs e)
        {
            // 모드 토글
            _isBarMode = !_isBarMode;

            // 버튼 텍스트 변경
            ToggleChartButton.Content = _isBarMode
                ? "파이 차트"
                : "막대 차트";

            // 현재 필터 기준으로 차트만 다시 렌더
            LoadChart(_currentFilter);
        }

        private void Button_Add_Click(object sender, RoutedEventArgs e)
        {
            // … 계정 추가 로직 …
        }

        private void Button_Delete_Click(object sender, RoutedEventArgs e)
        {
            // … 계정 삭제 로직 …
        }
    }

    public class UsageItemViewModel
    {
        public string DriveName { get; set; }
        public string TotalDisplay { get; set; }
        public string UsedDisplay { get; set; }
        public int UsedPercent { get; set; }
    }
}

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;

namespace OverCloud.transfer_manager
{
    public class TransferItemViewModel : INotifyPropertyChanged
    {
        private string _fileName;
        private string _status;
        private int _progress;
        private string _localPath;
        private double _animatedProgress;
        private DispatcherTimer _animationTimer;

        private DispatcherTimer _fakeProgressTimer;
        private DateTime _startTime;
        private double _expectedDuration; // 초 단위
        private const int _maxFakeProgress = 95;

        public double FileSizeMB { get; set; } // 업로드할 파일 크기

        public string FileName
        {
            get => _fileName;
            set { _fileName = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// 상태 (예: 대기 중, 다운로드 중, 완료, 실패, 오류)
        /// </summary>
        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// 진행률 (0~100)
        /// </summary>
        public int Progress
        {
            get => _progress;
            set 
            { 
                if(_progress != value)
                {
                    _progress = value;
                    OnPropertyChanged();
                    StartSmoothAnimation(); // Progress 변경 시마다 애니메이션 적용
                }

             }
        }

        public double AnimatedProgress
        {
            get => _animatedProgress;
            set
            {
                _animatedProgress = value;
                OnPropertyChanged();
            }
        }

        private void StartSmoothAnimation()
        {

            _animationTimer?.Stop(); // 기존 타이머 멈춤

            _animationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16)
            };
            _animationTimer.Tick += AnimationTick;
            _animationTimer.Start();
        }

        private void AnimationTick(object? sender, EventArgs e)
        {
            double diff = Progress - AnimatedProgress;

            if (Math.Abs(diff) < 0.5)
            {
                AnimatedProgress = Progress;
                _animationTimer.Stop();
            }
            else
            {
                AnimatedProgress += diff * 0.3;
            }
        }


        public void StartFakeProgress(double expectedSeconds)
        {
            Progress = 0;
            _expectedDuration = expectedSeconds;
            _startTime = DateTime.Now;

            _fakeProgressTimer?.Stop();
            _fakeProgressTimer = new DispatcherTimer 
            { 
                Interval = TimeSpan.FromMilliseconds(16) 
            };
            
            _fakeProgressTimer.Tick += (s, e) =>
            {
                var elapsed = (DateTime.Now - _startTime).TotalSeconds;
                var ratio = elapsed / _expectedDuration;
                var targetProgress = Math.Min(_maxFakeProgress, ratio * _maxFakeProgress);
                Progress = (int)targetProgress;

                if (ratio >= 1.0 || Progress >= _maxFakeProgress)
                {
                    Progress = _maxFakeProgress;
                    _fakeProgressTimer.Stop();
                }
            };

            _fakeProgressTimer.Start();
        }

        public void CompleteUpload()
        {
            _fakeProgressTimer?.Stop();
            _animationTimer?.Stop();
            Progress = 100;
            AnimatedProgress = 100;
            Status = "완료";
        }

        /// <summary>
        /// 저장될 로컬 경로
        /// </summary>
        public string LocalPath
        {
            get => _localPath;
            set { _localPath = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

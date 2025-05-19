using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OverCloud.transfer_manager
{
    public class TransferItemViewModel : INotifyPropertyChanged
    {
        private string _fileName;
        private string _status;
        private int _progress;
        private string _localPath;

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
            set { _progress = value; OnPropertyChanged(); }
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

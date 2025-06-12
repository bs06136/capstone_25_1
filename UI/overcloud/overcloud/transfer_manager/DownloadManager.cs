using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using OverCloud.Services.FileManager;
using OverCloud.transfer_manager;

namespace overcloud.transfer_manager
{
    public class DownloadManager
    {
        private readonly ObservableCollection<TransferItemViewModel> _downloads = new();
        private readonly BlockingCollection<(TransferItemViewModel Item, DownloadTaskInfo Task)> _queue = new();
        private readonly SemaphoreSlim _semaphore = new(2);
        private readonly FileDownloadManager _fileDownloadManager;

        public ObservableCollection<TransferItemViewModel> Downloads => _downloads;

        public DownloadManager(FileDownloadManager fileDownloadManager)
        {
            _fileDownloadManager = fileDownloadManager;

            Task.Run(ProcessQueue);
        }

        // ✅ 기존 EnqueueDownloads 시그니처 유지
        public void EnqueueDownloads(List<(int FileID, string FileName, string CloudFileId, int CloudStorageNum, string LocalPath, bool IsDistributed, ulong FileSize)> files, string user_id)
        {
            foreach (var file in files)
            {
                var item = new TransferItemViewModel
                {
                    FileName = file.FileName,
                    Status = "대기 중",
                    Progress = 0,
                    LocalPath = file.LocalPath
                };

                System.Windows.Application.Current.Dispatcher.Invoke(() => _downloads.Add(item));

                // ✅ 내부에서 DownloadTaskInfo로 변환해서 큐에 삽입
                var taskInfo = new DownloadTaskInfo(
                    file.FileID, file.FileName, file.CloudFileId, file.CloudStorageNum, file.LocalPath, file.IsDistributed, user_id, file.FileSize
                );

                _queue.Add((item, taskInfo));
            }
        }

        private async Task ProcessQueue()
        {
            foreach (var (item, task) in _queue.GetConsumingEnumerable())
            {
                await _semaphore.WaitAsync();
                _ = ProcessDownload(item, task);
            }
        }

        private async Task ProcessDownload(TransferItemViewModel item, DownloadTaskInfo file)
        {
            try
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() => { 
                    item.Status = "다운로드 중";
                    // 파일 크기로 예상 업로드 시간 계산 (20MB/sec 기준)

                    ulong fileSizeBytes = file.FileSize;
                    double fileSizeMB = fileSizeBytes / (1024.0 * 1024.0);
                    double expectedSeconds = Math.Max(3, fileSizeMB / 10.0); // 최소 3초 보장
                    item.StartFakeProgress(expectedSeconds);
                });

                if (file.IsDistributed)
                    await _fileDownloadManager.DownloadAndMergeFile(file.FileID, file.LocalPath, file.UserId, file.CloudStorageNum);
                else
                    await _fileDownloadManager.DownloadFile(file.UserId, file.CloudFileId, file.CloudStorageNum, file.LocalPath);

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    
                    item.CompleteDownload();
                    App.TransferManager.Completed.Add(item);
                });
            }
            catch (Exception ex)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() => { item.Status = "오류: " + ex.Message; });
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }

    // ✅ 내부 전용 TaskInfo 정의
    public record DownloadTaskInfo(
        int FileID, string FileName, string CloudFileId, int CloudStorageNum,
        string LocalPath, bool IsDistributed, string UserId, ulong FileSize);
}

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DB.overcloud.Models;
using System.Windows;
using OverCloud.Services.FileManager;
using OverCloud.transfer_manager;

namespace overcloud.transfer_manager
{
    public class DownloadManager
    {
        private readonly ObservableCollection<TransferItemViewModel> _downloads = new();
        private readonly SemaphoreSlim _semaphore = new(2); // 최대 2개 동시 다운로드

        public ObservableCollection<TransferItemViewModel> Downloads => _downloads;

        private FileDownloadManager _fileDownloadManager;

        public DownloadManager(FileDownloadManager fileDownloadManager)
        {
            _fileDownloadManager = fileDownloadManager;
        }

        public void EnqueueDownloads(List<(string FileName, string CloudFileId, int CloudStorageNum, string LocalPath)> files, string user_id)
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

                // UI 스레드에서 다운로드 목록에 추가
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    _downloads.Add(item);
                });

                // 다운로드 비동기 처리
                Task.Run(async () =>
                {
                    await _semaphore.WaitAsync();
                    try
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            item.Status = "다운로드 중";
                            item.Progress = 0;
                        });

                        // 실제 다운로드 실행
                        await _fileDownloadManager.DownloadFile(
                            userId: user_id,
                            cloudFileId: file.CloudFileId,
                            CloudStorageNum: file.CloudStorageNum,
                            savePath: file.LocalPath
                        );

                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            item.Status = "완료";
                            item.Progress = 100;

                            // 완료 목록에 추가
                            App.TransferManager.Completed.Add(item);
                        });
                    }
                    catch (Exception ex)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            item.Status = "오류: " + ex.Message;
                        });
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                });
            }
        }
    }
}

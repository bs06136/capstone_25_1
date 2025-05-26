using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DB.overcloud.Models;
using OverCloud.Services.FileManager;
using overcloud.transfer_manager;
using overcloud;
using OverCloud.Services;

namespace OverCloud.transfer_manager
{
    public class UploadManager
    {
        private readonly ObservableCollection<TransferItemViewModel> _uploads = new();
        private readonly SemaphoreSlim _semaphore = new(2); // 최대 2개 동시 다운로드
        public ObservableCollection<TransferItemViewModel> Uploads => _uploads;

        private readonly FileUploadManager _fileUploadManager;
        private readonly CloudTierManager _cloudTierManager;

        public UploadManager(FileUploadManager fileUploadManager, CloudTierManager cloudTierManager)
        {
            _fileUploadManager = fileUploadManager;
            _cloudTierManager = cloudTierManager;
        }

        public void EnqueueUploads(List<(string FileName, string FilePath, int ParentFolderId)> files, string user_id)
        {
            foreach (var file in files)
            {
                var item = new TransferItemViewModel
                {
                    FileName = file.FileName,
                    Status = "대기 중",
                    Progress = 0,
                    LocalPath = file.FilePath
                };

                // UI 쓰레드에서 리스트에 추가
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    _uploads.Add(item);
                });

                // 비동기 업로드 시작
                Task.Run(async () =>
                {
                    await _semaphore.WaitAsync();
                    try
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            item.Status = "업로드 중";
                            item.Progress = 0;
                        });

                        ulong fileSize = (ulong)new FileInfo(file.FilePath).Length;

                        var bestStorage = _cloudTierManager.SelectBestStorage(fileSize / 1024, user_id); //byte -> kb단위로 전달


                        bool result;

                        if (bestStorage != null)
                        {
                            result = await _fileUploadManager.file_upload(file.FilePath, file.ParentFolderId, user_id);

                        }
                        else
                        {
                            Console.WriteLine("분산저장 시작");
                            Console.WriteLine(file.ParentFolderId);
                            result = await _fileUploadManager.Upload_Distributed(file.FilePath, file.ParentFolderId, user_id);
                        }


                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            item.Status = result ? "완료" : "실패";
                            item.Progress = result ? 100 : 0;

                            if (result)
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
                });
            }
        }

        public void EnqueueUpload(UploadTaskInfo task, string user_id)
        {
            EnqueueUploads(new List<(string FileName, string FilePath, int ParentFolderId)>
            {
                (Path.GetFileName(task.LocalPath), task.LocalPath, task.FolderId)
            }, user_id);
        }
    }
}

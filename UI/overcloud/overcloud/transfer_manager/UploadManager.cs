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

namespace OverCloud.transfer_manager
{
    public class UploadManager
    {
        private readonly ObservableCollection<TransferItemViewModel> _uploads = new();
        public ObservableCollection<TransferItemViewModel> Uploads => _uploads;

        private readonly FileUploadManager _fileUploadManager;

        public UploadManager(FileUploadManager fileUploadManager)
        {
            _fileUploadManager = fileUploadManager;
        }

        public void EnqueueUploads(List<(string FileName, string FilePath, int ParentFolderId)> files)
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
                    try
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            item.Status = "업로드 중";
                            item.Progress = 0;
                        });

                        bool result = await _fileUploadManager.file_upload(file.FilePath, file.ParentFolderId);

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

        public void EnqueueUpload(UploadTaskInfo task)
        {
            EnqueueUploads(new List<(string FileName, string FilePath, int ParentFolderId)>
            {
                (Path.GetFileName(task.LocalPath), task.LocalPath, task.FolderId)
            });
        }
    }
}

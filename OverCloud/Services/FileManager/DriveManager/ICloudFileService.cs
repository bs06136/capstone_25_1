using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DB.overcloud.Models;
using DB.overcloud.Repository;

namespace OverCloud.Services.FileManager.DriveManager
{


    //나중에 OnedriveService, DropboxService도 이 클래스를 implements받아서 사용.
    public interface ICloudFileService
    {
        //지금은 클라우드에 직접 들어가서 파일을 다운로드 해오는 형식.
        
        Task<bool> DownloadFileAsync(string userId, string cloudFileId, string savePath, int CloudStorageNum);

        Task<bool> DeleteFileAsync(int cloudStorageNum, string fileId, string userId);

        Task<string> UploadFileAsync(CloudStorageInfo bestStorage, string filePath);

        Task<(ulong total, ulong used)> GetDriveQuotaAsync(int cloudStorageNum);


        //나중에 분산저장 구현 시 청크로 분리된 메서드를 하나로 합해서 사용할 수 있도록 분리
        //Task<byte[]> DownloadChunkAsync(string fileId);
    }


}

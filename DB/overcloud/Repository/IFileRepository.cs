using DB.overcloud.Models;
using System.Collections.Generic;

namespace DB.overcloud.Repository
{
    public interface IFileRepository
    {
        List<CloudFileInfo> GetAllFileInfo();
        bool AddFile(CloudFileInfo file);
        bool DeleteFile(int fileId);
        CloudFileInfo GetFileById(int fileId);
        List<CloudFileInfo> all_file_list(int fileId);
        CloudFileInfo specific_file_info(int fileId);
        List<CloudFileInfo> GetAllFileInfo(string file_direc)
        bool IncrementDownloadCount(int fileId);
    }
}

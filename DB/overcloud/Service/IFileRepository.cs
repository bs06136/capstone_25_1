using DB.overcloud.Models;
using System.Collections.Generic;

namespace DB.overcloud.Service
{
    public interface IFileRepository
    {
        List<CloudFileInfo> GetAllFileInfo();
        bool AddFile(CloudFileInfo file);
        bool DeleteFile(int fileId);
        CloudFileInfo GetFileById(int fileId);
    }
}

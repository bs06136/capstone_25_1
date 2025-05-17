using DB.overcloud.Models;
using System.Collections.Generic;

namespace DB.overcloud.Repository
{
    public interface IFileRepository
    {
        List<CloudFileInfo> GetAllFileInfo(int fileId);
        bool addfile(CloudFileInfo file_info);
        bool DeleteFile(int fileId);
        CloudFileInfo GetFileById(int fileId);
        List<CloudFileInfo> all_file_list(int fileId);
        CloudFileInfo specific_file_info(int fileId);
        List<CloudFileInfo> GetAllFileInfo(string file_direc);
        bool add_folder(CloudFileInfo file_info);
        bool change_name(CloudFileInfo file_info);
        bool change_dir(CloudFileInfo file_info);


        //새롭게 추가한 것들.
        int AddFileAndReturnId(CloudFileInfo file_info);
        public List<CloudFileInfo> GetChunksByRootFileId(int rootFileId);
        public List<CloudFileInfo> GetFilesByStorageNum(int cloudStorageNum);
    }
}

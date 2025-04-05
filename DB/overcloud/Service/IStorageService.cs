using DB.overcloud.Models;
using System.Collections.Generic;

namespace DB.overcloud.Service
{
    public interface IStorageService
    {
        List<CloudStorageInfo> GetCloudsForUser(string userId); 
        bool AddCloudStorage(CloudStorageInfo info);
        bool DeleteCloudStorage(int cloudStorageNum);
        bool account_save(CloudStorageInfo one_cloud);
    }
}

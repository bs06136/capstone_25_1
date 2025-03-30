using overcloud.Models;
using System.Collections.Generic;

namespace DB.overcloud.Service
{
    public interface IStorageService
    {
        bool AddCloudInfo(CloudStorageInfo info);
        List<CloudStorageInfo> GetCloudsForUser(string userId);
        bool DeleteAllCloudsForAccount(string userId);
    }
}

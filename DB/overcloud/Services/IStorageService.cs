using DB.overcloud.Models;
using System.Collections.Generic;

namespace DB.overcloud.Services
{
    public interface IStorageService
    {
        bool AddCloudInfo(CloudStorageInfo info);
        List<CloudStorageInfo> GetCloudsForUser(string userId);
        bool DeleteAllCloudsForAccount(string userId);
    }
}

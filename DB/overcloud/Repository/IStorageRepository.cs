using DB.overcloud.Models;
using System.Collections.Generic;

namespace DB.overcloud.Repository
{
    public interface IStorageRepository
    {
        List<CloudStorageInfo> GetCloudsForUser(string userId); 
        bool AddCloudStorage(CloudStorageInfo info);
        bool DeleteCloudStorage(int cloudStorageNum);
        bool account_save(CloudStorageInfo one_cloud);
        bool UpdateRefreshToken(int cloudStorageNum, string refreshToken);
    }
}

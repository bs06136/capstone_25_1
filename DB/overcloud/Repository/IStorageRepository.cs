using DB.overcloud.Models;

namespace DB.overcloud.Repository
{
    public interface IStorageRepository
    {
        int GetOrCreateCloudStorageNum(string cloudType, string accountId);
        CloudStorageInfo GetCloud(int cloudStorageNum, string userId);
        bool AddCloudStorage(CloudStorageInfo info);
        bool DeleteCloudStorage(int cloudStorageNum, string userId);
        bool account_save(CloudStorageInfo one_cloud);
        bool UpdateRefreshToken(int cloudStorageNum, string userId, string refreshToken);
    }
}

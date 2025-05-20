using DB.overcloud.Models;

namespace DB.overcloud.Repository
{
    public interface IStorageRepository
    {
        CloudStorageInfo GetCloud(string accountId);
        bool AddCloudStorage(CloudStorageInfo info);
        bool DeleteCloudStorage(int cloudStorageNum);
        bool account_save(CloudStorageInfo one_cloud);
        bool UpdateRefreshToken(int cloudStorageNum, string refreshToken);
    }
}

using DB.overcloud.Models;
using System.Collections.Generic;

namespace DB.overcloud.Service
{
    public interface IStorageService // 오버클라우드 계정을 관리하는 인터페이스 레파지토리
    {
        List<CloudStorageInfo> GetCloudsForUser(string userId); 
        bool AddCloudStorage(CloudStorageInfo info);
        bool DeleteCloudStorage(int cloudStorageNum);
        bool account_save(CloudStorageInfo one_cloud);
    }
}

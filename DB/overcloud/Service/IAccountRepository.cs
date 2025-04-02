using DB.overcloud.Models;
using System.Collections.Generic;

namespace DB.overcloud.Service
{
    public interface IAccountRepository
    {
        bool InsertAccount(CloudAccountInfo account);
        List<CloudStorageInfo> GetAllCloudAccounts();
        bool DeleteAccountByUserNum(int userNum);
        void UpdateTotalStorageForUser(string userId);
    }
}

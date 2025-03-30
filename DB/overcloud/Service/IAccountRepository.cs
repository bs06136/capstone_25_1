using overcloud.Models;
using System.Collections.Generic;

namespace DB.overcloud.Service
{
    public interface IAccountRepository
    {
        bool InsertAccount(CloudAccountInfo account);
        List<CloudAccountInfo> GetAllAccounts();
        bool DeleteAccountByUserNum(int userNum);
        void UpdateTotalStorageForUser(string userId);
    }
}

using DB.overcloud.Models;
using System.Collections.Generic;

namespace DB.overcloud.Services
{
    public interface IAccountService
    {
        bool InsertAccount(CloudAccountInfo account);
        List<CloudAccountInfo> GetAllAccounts();
        bool DeleteAccountByUserNum(int userNum);
        void UpdateTotalStorageForUser(string userId);
    }
}

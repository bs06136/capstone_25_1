namespace DB.overcloud.Service
{
    using DB.overcloud.Models;
    using System.Collections.Generic;

    public interface IAccountRepository
    {
        bool InsertAccount(CloudAccountInfo account);
        List<CloudAccountInfo> GetAllAccounts();
        bool DeleteAccountByUserNum(int userNum);
        void UpdateTotalStorageForUser(string userId);
    }
}

using DB.overcloud.Models;
using System.Collections.Generic;

namespace DB.overcloud.Repository
{
    public interface IAccountRepository
    {
        bool InsertAccount(CloudAccountInfo account);
        List<CloudStorageInfo> GetAllAccounts(string ID);
        bool DeleteAccountById(string id);
        bool UpdateAccountUsage(string id, ulong totalSize, ulong usedSize);
    }
}

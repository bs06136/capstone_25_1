using DB.overcloud.Models;
using System.Collections.Generic;

namespace DB.overcloud.Repository
{
    public interface IAccountRepository
    {
        bool InsertAccount(CloudAccountInfo account);
        List<CloudStorageInfo> GetAllAccounts();
        bool DeleteAccountByUserNum(int userNum);
        bool UpdateAccountUsage(int userNum, ulong totalSize, ulong usedSize);
    }
}

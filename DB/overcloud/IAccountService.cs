using System.Collections.Generic;
using OverCloud.Models;

namespace OverCloud.Services
{
    public interface IAccountService
    {
        bool InsertAccount(CloudAccountInfo account);
        List<CloudAccountInfo> GetAllAccounts();
        bool DeleteAccountByUserNum(int userNum);
    }
}

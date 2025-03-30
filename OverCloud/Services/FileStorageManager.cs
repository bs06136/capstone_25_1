using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OverCloud.Data;

namespace OverCloud.Services
{
    public class FileStorageManager
    {
        private readonly IAccountRepository accountRepo;

        private FileStorageManager(IAccountRepository repo)
        {
            accountRepo = repo;
        }

        public void StoreFile(string filePath)
        {
            var fileSize = new FileInfo(filePath).Length;
            var accounts = accountRepo.GetAllAccounts();

            var available = accounts.FirstOrDefault(a => a.TotalSize - a.UsedSize >= fileSize);

            if (available == null)
            {
                Console.WriteLine("❌ 저장할 계정이 없습니다.");
                return;
            }

            Console.WriteLine($"✅ {filePath} → {available.ID} 계정에 저장!");
        }
    }

}

using DB.overcloud.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace DB.overcloud.Repository
{
    public class AccountRepository : IAccountRepository
    {
        private readonly string connectionString;
        private readonly IStorageRepository cloudService;

        public AccountRepository(string connStr)
        {
            connectionString = connStr;
            cloudService = new StorageRepository(connStr);
        }

        public bool InsertAccount(CloudAccountInfo account)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = "INSERT INTO Account (ID, password, total_size, used_size, is_shared) VALUES (@id, @pw, 0, 0, @is_shared)";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", account.ID);
            cmd.Parameters.AddWithValue("@pw", account.Password);
            cmd.Parameters.AddWithValue("@is_shared", account.IsShared ? 1 : 0);

            return cmd.ExecuteNonQuery() > 0;
        }

        public List<CloudStorageInfo> GetAllAccounts(string ID)
        {
            var result = new List<CloudStorageInfo>();

            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string storageQuery = "SELECT * FROM CloudStorageInfo WHERE ID = @id";

            using var cmd = new MySqlCommand(storageQuery, conn);
            cmd.Parameters.AddWithValue("@id", ID);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new CloudStorageInfo
                {
                    CloudStorageNum = Convert.ToInt32(reader["cloud_storage_num"]),
                    ID = reader["ID"].ToString(),
                    CloudType = reader["cloud_type"].ToString(),
                    AccountId = reader["account_id"].ToString(),
                    AccountPassword = reader["account_pw"].ToString(),
                    TotalCapacity = reader["total_capacity"] != DBNull.Value ? Convert.ToUInt64(reader["total_capacity"]) : 0,
                    UsedCapacity = reader["used_capacity"] != DBNull.Value ? Convert.ToUInt64(reader["used_capacity"]) : 0,
                    RefreshToken = reader["refresh_token"]?.ToString(),
                    ClientId = reader["client_id"]?.ToString(),
                    ClientSecret = reader["client_secret"]?.ToString()
                });
            }

            return result;
        }

        public bool DeleteAccountById(string id)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string deleteQuery = "DELETE FROM Account WHERE ID = @id";
            using var deleteCmd = new MySqlCommand(deleteQuery, conn);
            deleteCmd.Parameters.AddWithValue("@id", id);
            bool result = deleteCmd.ExecuteNonQuery() > 0;

            if (result)
            {
                // cloudService.DeleteAllCloudsForAccount(id);  // 협업 클라우드 동기화 시 사용
            }

            return result;
        }
        
        public bool UpdateAccountUsage(string id, ulong totalSize, ulong usedSize)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = "UPDATE Account SET total_size = @total_size, used_size = @used_size WHERE ID = @id";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@total_size", totalSize);
            cmd.Parameters.AddWithValue("@used_size", usedSize);
            cmd.Parameters.AddWithValue("@id", id);

            return cmd.ExecuteNonQuery() > 0;
        }
    }
}

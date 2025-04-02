using DB.overcloud.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace DB.overcloud.Service
{
    public class AccountRepository : IAccountRepository
    {
        private readonly string connectionString;
        private readonly IStorageService cloudService;

        public AccountRepository(string connStr)
        {
            connectionString = connStr;
            cloudService = new StorageService(connStr);
        }

        public bool InsertAccount(CloudAccountInfo account)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = "INSERT INTO Account (username, ID, password, total_size, used_size) VALUES (@username, @id, @pw, 0, 0)";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@username", account.Username);
            cmd.Parameters.AddWithValue("@id", account.ID);
            cmd.Parameters.AddWithValue("@pw", account.Password);

            return cmd.ExecuteNonQuery() > 0;
        }

        public List<CloudStorageInfo> GetAllAccounts()
        {
            var list = new List<CloudStorageInfo>();

            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = "SELECT * FROM CloudStorageInfo";
            using var cmd = new MySqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new CloudStorageInfo
                {
                    CloudStorageNum = Convert.ToInt32(reader["cloud_storage_num"]),
                    UserNum = Convert.ToInt32(reader["user_num"]),
                    CloudType = reader["cloud_type"].ToString(),
                    AccountId = reader["account_id"].ToString(),
                    AccountPassword = reader["account_password"].ToString(),
                    TotalSize = (int)Convert.ToUInt64(reader["total_size"]),
                    UsedSize = (int)Convert.ToUInt64(reader["used_size"])
                });
            }

            return list;
        }

        public bool DeleteAccountByUserNum(int userNum)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string getIdQuery = "SELECT ID FROM Account WHERE user_num = @num";
            using var getIdCmd = new MySqlCommand(getIdQuery, conn);
            getIdCmd.Parameters.AddWithValue("@num", userNum);
            var id = getIdCmd.ExecuteScalar()?.ToString();
            if (id == null) return false;

            string deleteQuery = "DELETE FROM Account WHERE user_num = @num";
            using var deleteCmd = new MySqlCommand(deleteQuery, conn);
            deleteCmd.Parameters.AddWithValue("@num", userNum);
            bool result = deleteCmd.ExecuteNonQuery() > 0;

            if (result)
            {
                //cloudService.DeleteAllCloudsForAccount(id);
            }

            return result;
        }

        public void UpdateTotalStorageForUser(string userId)
        {
            var clouds = cloudService.GetCloudsForUser(userId);
            ulong total = 0, used = 0;

            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = "UPDATE Account SET total_size = @t, used_size = @u WHERE ID = @id";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@t", total);
            cmd.Parameters.AddWithValue("@u", used);
            cmd.Parameters.AddWithValue("@id", userId);

            cmd.ExecuteNonQuery();
        }
    }
}

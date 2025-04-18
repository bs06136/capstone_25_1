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
                    TotalCapacity = Convert.ToInt32(reader["total_capacity"]),
                    UsedCapacity = Convert.ToInt32(reader["used_capacity"]),
                    RefreshToken = reader["refresh_token"]?.ToString(),
                    ClientId = reader["client_id"]?.ToString(),
                    ClientSecret = reader["client_secret"]?.ToString()
                });
            }
            System.Diagnostics.Debug.WriteLine(list);
            System.Diagnostics.Debug.WriteLine("hello");
            return list;
        }

        public bool DeleteAccountByUserNum(int userNum)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string getIdQuery = "SELECT ID FROM Account WHERE user_num = @user_num";
            using var getIdCmd = new MySqlCommand(getIdQuery, conn);
            getIdCmd.Parameters.AddWithValue("@user_num", userNum);
            var id = getIdCmd.ExecuteScalar()?.ToString();
            if (id == null) return false;

            string deleteQuery = "DELETE FROM Account WHERE user_num = @user_num";
            using var deleteCmd = new MySqlCommand(deleteQuery, conn);
            deleteCmd.Parameters.AddWithValue("@user_num", userNum);
            bool result = deleteCmd.ExecuteNonQuery() > 0;

            if (result)
            {
                //cloudService.DeleteAllCloudsForAccount(id);
            }

            return result;
            }

        public bool UpdateAccountUsage(int userNum, ulong totalSize, ulong usedSize)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = "UPDATE Account SET total_size = @total_size, used_size = @used_size WHERE user_num = @user_num";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@total_size", totalSize);
            cmd.Parameters.AddWithValue("@used_size", usedSize);
            cmd.Parameters.AddWithValue("@user_num", userNum);

            return cmd.ExecuteNonQuery() > 0;
        }
    }
}

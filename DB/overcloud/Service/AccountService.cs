using DB.overcloud.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace DB.overcloud.Service
{
    public class AccountService : IAccountService
    {
        private readonly string connectionString;
        private readonly IStorageService cloudService;

        public AccountService(string connStr)
        {
            connectionString = connStr;
            cloudService = new StorageService(connStr);
        }

        public bool InsertAccount(CloudAccountInfo account)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = "INSERT INTO Account (ID, password, cloud_type, total_capacity, used_capacity) VALUES (@id, @pw, @type, 0, 0)";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", account.ID);
            cmd.Parameters.AddWithValue("@pw", account.Password);
            cmd.Parameters.AddWithValue("@type", account.CloudType);

            return cmd.ExecuteNonQuery() > 0;
        }

        public List<CloudAccountInfo> GetAllAccounts()
        {
            var list = new List<CloudAccountInfo>();

            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = "SELECT * FROM Account";
            using var cmd = new MySqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new CloudAccountInfo
                {
                    UserNum = Convert.ToInt32(reader["user_num"]),
                    ID = reader["ID"].ToString(),
                    Password = reader["password"].ToString(),
                    CloudType = reader["cloud_type"].ToString(),
                    TotalSize = Convert.ToInt64(reader["total_capacity"]),
                    UsedSize = Convert.ToInt64(reader["used_capacity"])
                });
            }

            return list;
        }

        public bool DeleteAccountByUserNum(int userNum)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            // ID 조회 먼저
            string getIdQuery = "SELECT ID FROM Account WHERE user_num = @num";
            using var getIdCmd = new MySqlCommand(getIdQuery, conn);
            getIdCmd.Parameters.AddWithValue("@num", userNum);
            var id = getIdCmd.ExecuteScalar()?.ToString();
            if (id == null) return false;

            // Account 삭제
            string deleteQuery = "DELETE FROM Account WHERE user_num = @num";
            using var deleteCmd = new MySqlCommand(deleteQuery, conn);
            deleteCmd.Parameters.AddWithValue("@num", userNum);
            bool result = deleteCmd.ExecuteNonQuery() > 0;

            if (result)
            {
                // 연결된 클라우드 정보도 삭제
                cloudService.DeleteAllCloudsForAccount(id);
            }

            return result;
        }

        public void UpdateTotalStorageForUser(string userId)
        {
            var clouds = cloudService.GetCloudsForUser(userId);
            long total = 0, used = 0;

            foreach (var c in clouds)
            {
                total += c.TotalCapacity;
                used += c.UsedCapacity;
            }

            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = "UPDATE Account SET total_capacity = @t, used_capacity = @u WHERE ID = @id";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@t", total);
            cmd.Parameters.AddWithValue("@u", used);
            cmd.Parameters.AddWithValue("@id", userId);

            cmd.ExecuteNonQuery();
        }
    }
}

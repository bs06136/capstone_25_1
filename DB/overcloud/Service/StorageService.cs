using overcloud.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace DB.overcloud.Service
{
    public class StorageService : IStorageService
    {
        private readonly string connectionString;

        public StorageService(string connStr)
        {
            connectionString = connStr;
        }

        public bool AddCloudInfo(CloudStorageInfo info)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = @"
                INSERT INTO CloudStorageInfo (account_user_num, total_capacity, used_capacity)
                VALUES (@userNum, @total, @used)";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@userNum", info.AccountUserNum);
            cmd.Parameters.AddWithValue("@total", info.TotalCapacity);
            cmd.Parameters.AddWithValue("@used", info.UsedCapacity);

            return cmd.ExecuteNonQuery() > 0;
        }

        public List<CloudStorageInfo> GetCloudsForUser(string userId)
        {
            var list = new List<CloudStorageInfo>();

            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            // 먼저 user_num 조회
            string getUserNumQuery = "SELECT user_num FROM Account WHERE ID = @id LIMIT 1";
            using var getUserCmd = new MySqlCommand(getUserNumQuery, conn);
            getUserCmd.Parameters.AddWithValue("@id", userId);
            object result = getUserCmd.ExecuteScalar();
            if (result == null) return list;

            int userNum = Convert.ToInt32(result);

            string query = "SELECT * FROM CloudStorageInfo WHERE account_user_num = @userNum";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@userNum", userNum);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new CloudStorageInfo
                {
                    Id = Convert.ToInt32(reader["id"]),
                    AccountUserNum = Convert.ToInt32(reader["account_user_num"]),
                    TotalCapacity = Convert.ToInt64(reader["total_capacity"]),
                    UsedCapacity = Convert.ToInt64(reader["used_capacity"])
                });
            }

            return list;
        }

        public bool DeleteAllCloudsForAccount(string userId)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            // 먼저 user_num 조회
            string getUserNumQuery = "SELECT user_num FROM Account WHERE ID = @id LIMIT 1";
            using var getUserCmd = new MySqlCommand(getUserNumQuery, conn);
            getUserCmd.Parameters.AddWithValue("@id", userId);
            object result = getUserCmd.ExecuteScalar();
            if (result == null) return false;

            int userNum = Convert.ToInt32(result);

            string query = "DELETE FROM CloudStorageInfo WHERE account_user_num = @userNum";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@userNum", userNum);

            return cmd.ExecuteNonQuery() > 0;
        }
    }
}

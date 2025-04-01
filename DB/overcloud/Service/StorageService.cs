using DB.overcloud.Models;
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

        public List<CloudStorageInfo> GetCloudsForUser(string userId)
        {
            var list = new List<CloudStorageInfo>();

            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = @"
                SELECT cs.* 
                FROM CloudStorageInfo cs
                JOIN Account a ON cs.user_num = a.user_num
                WHERE a.ID = @id";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", userId);
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

        public bool AddCloudStorage(CloudStorageInfo info)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = @"INSERT INTO CloudStorageInfo 
                (user_num, cloud_type, account_id, account_password, total_size, used_size) 
                VALUES 
                (@user_num, @type, @id, @pw, @total, @used)";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@user_num", info.UserNum);
            cmd.Parameters.AddWithValue("@type", info.CloudType);
            cmd.Parameters.AddWithValue("@id", info.AccountId);
            cmd.Parameters.AddWithValue("@pw", info.AccountPassword);
            cmd.Parameters.AddWithValue("@total", info.TotalSize);
            cmd.Parameters.AddWithValue("@used", info.UsedSize);

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool DeleteCloudStorage(int cloudStorageNum)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = "DELETE FROM CloudStorageInfo WHERE cloud_storage_num = @num";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@num", cloudStorageNum);

            return cmd.ExecuteNonQuery() > 0;
        }
    }
}

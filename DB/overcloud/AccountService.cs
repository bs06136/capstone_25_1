using MySql.Data.MySqlClient;
using OverCloud.Models;
using System;
using System.Collections.Generic;

namespace OverCloud.Services
{
    public class AccountService : IAccountService
    {
        private readonly string connectionString = "Server=localhost;Database=over_cloud;Uid=admin;Pwd=admin;";

        public List<CloudAccountInfo> GetAllAccounts()
        {
            List<CloudAccountInfo> accounts = new List<CloudAccountInfo>();

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT * FROM Account";

                using (var cmd = new MySqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        accounts.Add(new CloudAccountInfo
                        {
                            UserNum = Convert.ToInt32(reader["user_num"]),
                            ID = reader["ID"].ToString(),
                            Password = reader["password"].ToString(),
                            CloudType = reader["cloud_type"].ToString()
                        });
                    }
                }
            }

            return accounts;
        }

        public bool AccountExists(CloudAccountInfo account)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "SELECT COUNT(*) FROM Account WHERE ID = @id AND password = @password AND cloud_type = @cloudType";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", account.ID);
                    cmd.Parameters.AddWithValue("@password", account.Password);
                    cmd.Parameters.AddWithValue("@cloudType", account.CloudType);

                    long count = (long)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
        }

        public bool InsertAccount(CloudAccountInfo account)
        {
            if (AccountExists(account))
            {
                Console.WriteLine("⚠️ 이미 동일한 계정이 존재합니다.");
                return false;
            }

            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "INSERT INTO Account (ID, password, cloud_type) VALUES (@id, @password, @cloudType)";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", account.ID);
                    cmd.Parameters.AddWithValue("@password", account.Password);
                    cmd.Parameters.AddWithValue("@cloudType", account.CloudType);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool DeleteAccountByUserNum(int userNum)
        {
            using (var conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string query = "DELETE FROM Account WHERE user_num = @userNum";

                using (var cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@userNum", userNum);
                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }
    }
}

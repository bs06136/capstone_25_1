using MySql.Data.MySqlClient;
using OverCloud.Data;
using System.Collections.Generic;

public class AccountRepository :IAccountRepositiory
{
    private readonly DatabaseHelper dbHelper = new DatabaseHelper();

  //  private const string ConnStr = "Server=localhost;Database=overcloud;Uid=admin;Pwd=admin;";

    // 계정 추가
    public bool InsertAccount(CloudAccountInfo account)
    {
        string query = "INSERT INTO Account (ID, password, cloud_type) VALUES (@id, @password, @cloudType)";

        using (var conn = dbHelper.GetConnection())
        {
            conn.Open();
            using (var cmd = new MySqlCommand(query, conn))
            {
//                cmd.Parameters.AddWithValue("@usernum", account.UserNum);
                cmd.Parameters.AddWithValue("@id", account.ID);
                cmd.Parameters.AddWithValue("@password", account.Password);
                cmd.Parameters.AddWithValue("@cloudType", account.CloudType);

                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }


    // 계정 삭제
    public bool DeleteAccountByUserNum(int userNum)
    {
        string query = "DELETE FROM Account WHERE user_num = @userNum";

        using (var conn = dbHelper.GetConnection())
        {
            conn.Open();
            using (var cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@userNum", userNum);
                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }


    //  모든 계정 정보 조회
    public List<CloudAccountInfo> GetAllAccounts()
    {
        List<CloudAccountInfo> accounts = new List<CloudAccountInfo>();
        string query = "SELECT * FROM Account";

        using (var conn = dbHelper.GetConnection())
        {
            conn.Open();
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

}

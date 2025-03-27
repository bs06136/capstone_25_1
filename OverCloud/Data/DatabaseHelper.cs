using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace OverCloud.Data
{ //DB 연결 클래스
    public class DatabaseHelper
    {
        private static string connectionString = "Server=localhost;Database=overcloud;Uid=admin;Pwd=admin;";
        // 연결 객체 가져오기
        public MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }
    }
}
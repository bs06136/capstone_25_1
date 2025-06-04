using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DB.overcloud.Models;
using DB.overcloud.Repository;
using OverCloud.Services.FileManager.DriveManager;
using System.Security.Cryptography;


namespace OverCloud.Services
{
    public class PasswordHasher
    {
        // 비밀번호와 salt를 받아 SHA256 해시를 반환
        public static string HashPassword(string userId, string password, string salt)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                string combined = password+ salt;
                byte[] bytes = Encoding.UTF8.GetBytes(combined);
                byte[] hash = sha256.ComputeHash(bytes);

                // 해시를 16진수 문자열로 변환
                StringBuilder result = new StringBuilder();
                foreach (byte b in hash)
                {
                    result.Append(b.ToString("x2"));
                }

                return result.ToString();
            }
        }

        // 무작위 salt 생성 (예: 사용자 등록 시)
        public static string GenerateSalt(int size = 32)
        {
            byte[] saltBytes = new byte[size];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        //public static string HashPassword_verify(string userId, string password, string salt)
        //{
        //    using (SHA256 sha256 = SHA256.Create())
        //    {
        //        string combined = password + salt;
        //        byte[] bytes = Encoding.UTF8.GetBytes(combined);
        //        byte[] hash = sha256.ComputeHash(bytes);

        //        // 해시를 16진수 문자열로 변환
        //        StringBuilder result = new StringBuilder();
        //        foreach (byte b in hash)
        //        {
        //            result.Append(b.ToString("x2"));
        //        }

        //        return result.ToString();
        //    }
        //}
    }

}
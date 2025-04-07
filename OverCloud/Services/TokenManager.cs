using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DB.overcloud.Models;
using DB.overcloud.Repository;

namespace OverCloud.Services
{
    public class TokenManager
    {
        private readonly Dictionary<string, AccessTokenProvider> providers = new(); //전역적으로 적용?

        public void AddProvider(string userId, string refreshToken, string clientId, string clientSecret)
        {
            providers[userId] = new AccessTokenProvider(refreshToken, clientId, clientSecret);
        }

        public async Task<string> GetAccessTokenAsync(string userId)
        {
            if (!providers.ContainsKey(userId))
                throw new Exception($"❌ 토큰 정보가 없는 사용자: {userId}");

            return await providers[userId].GetAccessTokenAsync();
        }
    }

}
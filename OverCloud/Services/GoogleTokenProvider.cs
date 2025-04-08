using DB.overcloud.Models;
using System.Text.Json;
using System.Net.Http.Headers;
using System.Net.Http;
using DB.overcloud.Repository;


public class GoogleTokenProvider
{
    private readonly Dictionary<string, (string token, DateTime expiry)> tokenCache = new();

    public async Task<string> GetAccessTokenAsync(CloudStorageInfo cloud)
    {
        if (cloud == null)
            throw new ArgumentNullException(nameof(cloud));

        if (string.IsNullOrEmpty(cloud.RefreshToken) ||
            string.IsNullOrEmpty(cloud.ClientId) ||
            string.IsNullOrEmpty(cloud.ClientSecret))
        {
            throw new InvalidOperationException("CloudStorageInfo에 필요한 인증 정보가 없습니다.");
        }

        // 캐시에 유효한 토큰이 있다면 재사용
        if (tokenCache.TryGetValue(cloud.AccountId, out var cache))
        {
            if (DateTime.UtcNow < cache.expiry)
                return cache.token;
        }

        // AccessToken 갱신
        var newToken = await RequestNewAccessToken(cloud);
        tokenCache[cloud.AccountId] = newToken;

        return newToken.token;
    }

    private async Task<(string token, DateTime expiry)> RequestNewAccessToken(CloudStorageInfo cloud)
    {
        using var client = new HttpClient();

        var parameters = new Dictionary<string, string>
        {
            { "client_id", cloud.ClientId },
            { "client_secret", cloud.ClientSecret },
            { "refresh_token", cloud.RefreshToken },
            { "grant_type", "refresh_token" }
        };

        var response = await client.PostAsync("https://oauth2.googleapis.com/token", new FormUrlEncodedContent(parameters));
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        using var json = JsonDocument.Parse(content);

        string accessToken = json.RootElement.GetProperty("access_token").GetString();
        int expiresIn = json.RootElement.GetProperty("expires_in").GetInt32();

        DateTime expiry = DateTime.UtcNow.AddSeconds(expiresIn - 60); // 안전 여유

        return (accessToken, expiry);
    }
}

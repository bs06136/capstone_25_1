using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DB.overcloud.Models;
using DB.overcloud.Repository;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Util.Store;

namespace OverCloud.Services
{
    public static class GoogleAuthHelper
    {
        private const string CredentialFile = "C:\\key\\credential.json";

        public static async Task<(string email,string RefreshToken, string ClientId, string ClientSecret)> AuthorizeAsync()
        {

            using var stream = new FileStream(CredentialFile, FileMode.Open, FileAccess.Read);
            var secrets = GoogleClientSecrets.FromStream(stream).Secrets;

            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                secrets,
                new[] { DriveService.Scope.Drive },
                "userId",
                CancellationToken.None,
                new FileDataStore("Tokens", true)
            );

            // RefreshToken 추출
            var refreshToken = ((UserCredential)credential).Token.RefreshToken;
            var email = credential.UserId;

            return (email,refreshToken, secrets.ClientId, secrets.ClientSecret);
        }
    }

}
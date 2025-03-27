using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;

namespace OverCloud.Services
{
    //실제 API 대신 모의 데이터를 반환하는 클래스
    //나중에 진짜 API 연동으로 교체 가능하도록 구조화함
    public class CloudApiService
    {
        public (long totalSize, long usedSize) GetStorageInfo(CloudAccountInfo account)
        {
            switch (account.CloudType)
            {
                case "GoogleDrive":
                    return SimulateGoogleDrive(account);
                case "Dropbox":
                    return SimulateDropbox(account);
                case "OneDrive":
                    return SimulateOneDrive(account);
                default:
                    return (0, 0);
            }
        }

        private (long, long) SimulateGoogleDrive(CloudAccountInfo acc)
        {
            // 나중엔 Google Drive API 연동
            return (15_000_000_000L, 7_500_000_000L); // 예: 15GB 중 7.5GB 사용
        }

        private (long, long) SimulateDropbox(CloudAccountInfo acc)
        {
            return (2_000_000_000L, 1_200_000_000L);
        }

        private (long, long) SimulateOneDrive(CloudAccountInfo acc)
        {
            return (5_000_000_000L, 2_800_000_000L);
        }
    }

}

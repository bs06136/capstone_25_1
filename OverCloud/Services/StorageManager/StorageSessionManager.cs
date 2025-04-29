using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using DB.overcloud.Models;
using DB.overcloud.Repository;

namespace OverCloud.Services.StorageManager
{
    public static class StorageSessionManager
    {
        public static List<CloudQuotaInfo> Quotas { get; set; } = new List<CloudQuotaInfo>();

    }

}
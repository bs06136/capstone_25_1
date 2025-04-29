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
    public class CloudQuotaInfo
    {
        public int CloudStorageNum { get; set; }
        public string CloudType { get; set; }
        public int TotalCapacityMB { get; set; }
        public int UsedCapacityMB { get; set; }
    }

}
﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using DB.overcloud.Models;
using DB.overcloud.Repository;
using OverCloud.Services.FileManager.DriveManager;

namespace OverCloud.Services
{
    public class TokenProviderFactory
    {
      //  private readonly Dictionary<string, ICloudTokenProvider> providers;

        public GoogleTokenProvider CreateGoogleTokenProvider()
        {
            return new GoogleTokenProvider();
        }

        public OneDriveTokenRefresher CreateOneDriveTokenRefresher()
        {
            return new OneDriveTokenRefresher();
        }

        public DropboxTokenRefresher CreateDropboxTokenRefresher()
        {
            return new DropboxTokenRefresher();
        }
    }

}
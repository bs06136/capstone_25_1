using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Web;
using OverCloud.Services;
using OverCloud.transfer_manager;
using Microsoft.Win32;
using overcloud.Windows;

namespace overcloud
{
    public partial class App : System.Windows.Application
    {
        private LoginController _controller;
        public static TransferManager TransferManager { get; set; }

        public static List<(string userId, string cloudFileId, int fileId)> PendingDownloads { get; private set; } = new();

        private const string MutexName = "OverCloudSingleInstance";
        private const string PipeName = "OverCloudPipe";

        public static event Action DownloadRequestReceived;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            bool isNew;
            using (Mutex mutex = new Mutex(true, MutexName, out isNew))
            {
                if (isNew)
                {
                    StartPipeServer();
                    ParseArgs(e.Args); // 최초 실행시 파라미터도 누적만

                    _controller = new LoginController();
                    var loginWindow = new overcloud.Views.LoginWindow(_controller);
                    loginWindow.Show();
                }
                else
                {
                    Shutdown();
                }
            }
        }

        private void ParseArgs(string[] args)
        {
            if (args.Length > 0)
            {
                try
                {
                    string fullUrl = args[0];
                    if (fullUrl.StartsWith("overcloud://"))
                    {
                        int index = fullUrl.IndexOf("?link=");
                        if (index >= 0)
                        {
                            string encodedLinkParam = fullUrl.Substring(index + 6);
                            string linkParam = Uri.UnescapeDataString(encodedLinkParam);

                            string[] fileItems = linkParam.Split('|');
                            foreach (var item in fileItems)
                            {
                                var parts = item.Split(',');
                                if (parts.Length >= 3)
                                {
                                    string userId = parts[0];
                                    string cloudFileId = parts[1];
                                    int fileId = int.Parse(parts[2]);

                                    PendingDownloads.Add((userId, cloudFileId, fileId));
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("링크 파싱 실패: " + ex.Message);
                }
            }
        }

        private void StartPipeServer()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        using (var server = new NamedPipeServerStream(
                            PipeName,
                            PipeDirection.In,
                            NamedPipeServerStream.MaxAllowedServerInstances,
                            PipeTransmissionMode.Byte,
                            PipeOptions.Asynchronous))
                        {
                            server.WaitForConnection();

                            using (var reader = new StreamReader(server))
                            {
                                string incoming = reader.ReadLine();
                                if (!string.IsNullOrEmpty(incoming))
                                {
                                    ParseArgs(new[] { incoming });

                                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        DownloadRequestReceived?.Invoke();
                                    });
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            System.Windows.MessageBox.Show($"파이프 서버 오류: {ex.Message}");
                        });
                    }
                }
            });
        }
    }
}

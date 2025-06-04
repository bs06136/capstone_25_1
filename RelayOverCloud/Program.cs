using System;
using System.IO;
using System.IO.Pipes;
using System.Diagnostics;
using System.Threading;

namespace RelayOverCloud
{
    class Program
    {
        private const string PipeName = "OverCloudPipe";

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                return;
            }

            string fullUrl = args[0];

            bool pipeSent = false;

            for (int i = 0; i < 3; i++) // 최대 3회 재시도
            {
                try
                {
                    using (var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out))
                    {
                        client.Connect(500);  // 500ms 대기
                        using (var writer = new StreamWriter(client))
                        {
                            writer.WriteLine(fullUrl);
                            writer.Flush();
                            pipeSent = true;
                        }
                    }
                    break;
                }
                catch
                {
                    if (i == 0)
                    {
                        // 첫 연결 실패 → 메인 프로그램 실행 시도
                        StartMainProgram();
                    }
                    Thread.Sleep(300); // 대기 후 재시도
                }
            }

            if (!pipeSent)
            {
                Console.WriteLine("Pipe communication failed.");
            }
        }

        private static void StartMainProgram()
        {
            try
            {
                string relayPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string relayDirectory = Path.GetDirectoryName(relayPath);

                // Main 프로그램이 Relay와 같은 폴더에 있다고 가정
                string exePath = Path.Combine(relayDirectory, "OverCloud.exe");

                Process.Start(exePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Main program start failed: " + ex.Message);
            }
        }

    }
}

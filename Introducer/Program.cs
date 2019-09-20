using Config.Net;
using Fleck;
using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Net;
using Serilog;

namespace Introducer
{
    class Program
    {
        static void SetupLogging()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("introducer.log",
                    rollingInterval: RollingInterval.Day,
                    rollOnFileSizeLimit: true)
                .CreateLogger();
        }

        static IConfig LoadConfig()
        {
            return new ConfigurationBuilder<IConfig>()
                  .UseJsonConfig("config.json")
                  .Build();
        }

        static IWebSocketServer CreateWebsocketServer(IConfig config)
        {
            var currentDir = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
            var keyPath = Path.IsPathRooted(currentDir) ? config.CertificatePath : Path.Join(currentDir, config.CertificatePath);

            FleckLog.Level = LogLevel.Info;
            var server = new WebSocketServer($"wss://0.0.0.0:{config.Port}", false);
            server.EnabledSslProtocols = SslProtocols.Tls13 | SslProtocols.Tls12;
            server.RestartAfterListenError = true;
            server.Certificate = new X509Certificate2(keyPath, "", X509KeyStorageFlags.MachineKeySet);

            return server;
        }

        static void Main(string[] args)
        {
            SetupLogging();
            var config = LoadConfig();
            var clients = new ClientManager();
            var server = CreateWebsocketServer(config);

            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    if (clients.Count >= config.MaxWebsocketConnections)
                    {
                        Log.Debug($"{socket.ConnectionInfo.ClientIpAddress}: [Connected] Over connection limit with {clients.Count} sockets connected. Dropping connection.");
                        socket.Close();
                    } else
                    {
                        Log.Debug($"{socket.ConnectionInfo.ClientIpAddress}: [Connected].");

                        var client = new Client() { LanIp = IPAddress.None, Role = ClientRole.Unknown, Socket = socket };
                        Log.Debug($"{client.Id}: [Register].");
                        clients.Register(client);
                    }
                };

                socket.OnClose = () =>
                {
                    Log.Debug($"{socket.ConnectionInfo.ClientIpAddress}: [Disconnected].");

                    var client = clients.FindById(socket.ConnectionInfo.Id);
                    Log.Debug($"{client.Id}: [Unregister].");
                    clients.Unregister(client);
                };

                socket.OnMessage = message =>
                {
                    var client = clients.FindById(socket.ConnectionInfo.Id);

                    var splitMessage = message.Split('|');
                    var command = splitMessage.Length >= 1 ? splitMessage[0] : null;
                    var args = splitMessage.Length >= 2 ? splitMessage[1] : null;
                    var args2 = splitMessage.Length >= 3 ? splitMessage[2] : null;

                    switch (command)
                    {
                        case "update-role":
                            switch (args)
                            {
                                case "interpreter":
                                    IPAddress lanIp;
                                    if (!IPAddress.TryParse(args2, out lanIp))
                                    {
                                        Log.Debug($"{client.Id}: [Update Role -> Interpreter] Failed because LAN IP sent was not parseable.");
                                        return;
                                    }
                                    client.Role = ClientRole.Interpreter;
                                    client.LanIp = lanIp;
                                    Log.Debug($"{client.Id}: [Update Role] Registered as {client.Role}.");
                                    clients.IntroducePeers(client);
                                    break;
                                case "performer":
                                    client.Role = ClientRole.Performer;
                                    Log.Debug($"{client.Id}: [Update Role] Registered as {client.Role}.");
                                    clients.IntroducePeers(client);
                                    break;
                                default:
                                    Log.Debug($"{client.Id}: [Update Role] Failed to register: invalid role.");
                                    break;
                            }
                            break;
                    }
                };
            });

            string input;
            do
            {
                Console.Write("Type 'exit' to quit: ");
                input = Console.ReadLine();
            }
            while (input != "exit");
        }
    }
}

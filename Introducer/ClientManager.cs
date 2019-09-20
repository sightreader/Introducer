using Fleck;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Serilog;

namespace Introducer
{
    public class ClientManager
    {
        private IDictionary<Guid, Client> ClientsById { get; set; }
        private IDictionary<IPAddress, List<Client>> ClientsByWanIp { get; set; }

        public ClientManager()
        {
            ClientsById = new Dictionary<Guid, Client>();
            ClientsByWanIp = new Dictionary<IPAddress, List<Client>>();
        }

        public int Count
        {
            get
            {
                return ClientsById.Values.Count;
            }
        }

        public Client FindById(Guid clientId)
        {
            return ClientsById[clientId];
        }

        public void Register(Client client)
        {
            ClientsById[client.Id] = client;

            if (!ClientsByWanIp.TryGetValue(client.WanIp, out List<Client> clientsForWanIp))
            {
                clientsForWanIp = ClientsByWanIp[client.WanIp] = new List<Client>();
            }
            if (!clientsForWanIp.Contains(client))
            {
                clientsForWanIp.Add(client);
            }
        }

        public void Unregister(Client client)
        {
            ClientsById.Remove(client.Id);

            if (!ClientsByWanIp.TryGetValue(client.WanIp, out List<Client> clientsForWanIp))
            {
                return;
            }
            if (clientsForWanIp.Contains(client))
            {
                clientsForWanIp.Remove(client);
            }
        }

        public void IntroducePeers(Client client)
        {
            switch (client.Role)
            {
                case ClientRole.Interpreter:
                    {
                        var interpreter = client;
                        if (!ClientsByWanIp.TryGetValue(client.WanIp, out List<Client> clients))
                        {
                            return;
                        }

                        var performers = clients.FindAll(client => client.Role == ClientRole.Performer);
                        performers.ForEach(performer =>
                        {
                            var lanIpAddress = interpreter.LanIp.MapToIPv4().ToString();
                            performer.Socket.Send($"interpreter-ip|{lanIpAddress}");
                            Log.Debug($"[ Introduction] (interpreter) {performer.Id} -> (performer) {client.Id}: Interpreter LAN IP is {lanIpAddress}.");
                        });
                    }
                    break;
                case ClientRole.Performer:
                    {
                        var performer = client;

                        if (!ClientsByWanIp.TryGetValue(client.WanIp, out List<Client> clients))
                        {
                            return;
                        }

                        var interpreter = clients.Find(client => client.Role == ClientRole.Interpreter);

                        if (interpreter == null)
                        {
                            return;
                        }

                        var lanIpAddress = interpreter.LanIp.MapToIPv4().ToString();
                        performer.Socket.Send($"interpreter-ip|{lanIpAddress}");
                        Log.Debug($"[Introduction] (interpreter) {performer.Id} -> (performer) {client.Id}: Interpreter LAN IP is {lanIpAddress}.");
                    }
                    break;
                default:
                    return;
            }
        }
    }
}

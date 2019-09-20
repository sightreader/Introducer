using Fleck;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Introducer
{
    public enum ClientRole
    {
        Interpreter,
        Performer,
        /// <summary>
        /// The client's role has not been provided (e.g. the client has only just connected).
        /// </summary>
        Unknown
    }

    public class Client
    {
        public ClientRole Role { get; set; }

        /// <summary>
        /// The underlying websocket connection.
        /// </summary>
        public IWebSocketConnection Socket { get; set; }

        /// <summary>
        /// The client's provided IP address.
        /// 
        /// If this client's role is an interpreter, this IP will be exchanged to
        /// other performer clients under the same WAN IP.
        /// 
        /// If the client's role is a performer, no IP needs to be provided because
        /// the future connection will be outgoing from the performer to the interpreter.
        /// </summary>
        public IPAddress LanIp { get; set; }

        /// <summary>
        /// The client's publicly visible external IP address.
        /// 
        /// Clients are bucketed by their WAN IP.
        /// </summary>
        public IPAddress WanIp
        {
            get
            {
                if (_cachedWanIp == null)
                {
                    _cachedWanIp = IPAddress.Parse(Socket.ConnectionInfo.ClientIpAddress);
                }

                return _cachedWanIp;
            }
        }

        private IPAddress _cachedWanIp;

        public Guid Id
        {
            get
            {
                return Socket.ConnectionInfo.Id;
            }
        }

        public bool Equals(Client x, Client y)
        {
            if (Object.ReferenceEquals(x, y)) return true;

            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.Socket.ConnectionInfo.Id == y.Socket.ConnectionInfo.Id;
        }

        public int GetHashCode(Client client)
        {
            if (Object.ReferenceEquals(client, null)) return 0;

            return client.Socket.ConnectionInfo.Id.GetHashCode();
        }
    }
}

using Config.Net;
using System;
using System.Collections.Generic;
using System.Text;

namespace Introducer
{
    public interface IConfig
    {
        [Option(DefaultValue = 55367)]
        int Port { get; set; }

        /// <summary>
        /// Can be a relative or absolute path to a .pfx certificate file.
        /// 
        /// Other formats (.crt, .pem) will not work.
        /// </summary>
        [Option(DefaultValue = "assets\\cert.pfx")]
        string CertificatePath { get; set; }


        /// <summary>
        /// The maximum number of incoming websocket connections allowed before
        /// connections are dropped.
        /// </summary>
        [Option(DefaultValue = 1000)]
        int MaxWebsocketConnections { get; set; }
    }
}

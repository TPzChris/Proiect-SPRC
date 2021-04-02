using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Connection
{
    interface IConnection
    {
        bool close();
        bool close(Socket sock);
        bool connect(IPEndPoint remoteEndpoint);
        bool reserve(int port);
        bool send(byte[] data);
        bool sendBySpecificSocket(byte[] data, Socket sockAddr);
    }
}

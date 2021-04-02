using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Connection
{
    public class ConnectionObject
    {
        public byte[] workingBytes;
        public Socket clientSocket;
    }

    public class ReceiveCompletedEventArgs : EventArgs
    {
        public byte[] data { get; set; }
        public Socket remoteSock { get; set; }
        public ReceiveCompletedEventArgs(byte[] data, Socket workSock)
        {
            this.data = data;
            this.remoteSock = workSock;
        }
    }

    public class ExceptionRaiseEventArgs : EventArgs
    {
        public Exception raisedException;
        public ExceptionRaiseEventArgs(Exception exc)
        {
            this.raisedException = exc;
        }
    }
}

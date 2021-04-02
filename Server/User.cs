using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    public class User
    {
        public enum StatusType
        {
            UsernameInvalid,
            Connecting,
            Connected
        }

        public string Username { get; set; }
        public StatusType Status { get; set; }
        public string IncompleteMessage { get; set; }

        public Socket socket { get; set; }


        public User(string username)
        {
            Username = username;
            Status = StatusType.Connecting;
            IncompleteMessage = null;
            socket = null;
        }

        public User()
        {
            Username = null;
            Status = StatusType.UsernameInvalid;
            IncompleteMessage = null;
            socket = null;
        }
    }
}

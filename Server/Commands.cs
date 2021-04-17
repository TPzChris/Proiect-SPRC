using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Server.Program;

namespace Server
{
    public class Commands
    {
        public const string CommandDelim = "|";
        public const string SubCommandDelim = ":";
        public const string EndMessageDelim = "<##>";


        public const string ValidateUsername = "ValUnam";
        public const string EndPoint = "EndPoint";
        public const string Connect = "Cnct";
        public const string Disconnect = "Discon";
        public const string Logout = "Lgout";


        public const string PublicMessage = "PubMsg";
        public const string PrivateMessage = "PrivMsg";

        public const string UserList = "UsLst";

        public const string MalformedCommand = "MalComm";
        public const string InvalidRequest = "IReq";

        public const string Request = "Req";
        public const string Deny = "Den";
        public const string Accept = "Acc";
        public const string Add = "Add";
        public const string Remove = "Rem";
        public const string None = "None";

        public const string UserCount = "Users";
        public const string FirstIsReady = "FirstIsReady";
        public const string SecondIsReady = "SecondIsReady";
        public const string StartGame = "StartGame";

        public static byte[] CreateMessage(string command, string subcommand, string data)
        {
            if (command == null) return null;
            if (subcommand == null) subcommand = None;
            if (data == null) data = None;

            return Encoding.Unicode.GetBytes(command + CommandDelim + subcommand + CommandDelim + data + EndMessageDelim);
        }

        public static byte[] CreatePrivateMessage(string command, string subcommand, string data, List<String> list)
        {
            if (command == null) return null;
            if (subcommand == null) subcommand = None;
            if (data == null) data = None;
            string receivers = "";

            if(list.Count > 1)
            {
                list.ForEach(x => receivers += x + "/");
            }
            else 
            {
                receivers = list.First();
            }
            selectedUsers = list;

            return Encoding.Unicode.GetBytes(command + CommandDelim + receivers + CommandDelim + data + EndMessageDelim);
        }

        public static Message DecodeMessage(string message)
        {
            string[] parts = message.Split(new string[] { CommandDelim }, StringSplitOptions.None);

            if(parts.Length != 3) return new Message(MalformedCommand, None, None);
            return new Message(parts[0], parts[1], parts[2]);
        }

        public class Message
        {
            public string Command { get; set; }
            public string Subcommand { get; set; }
            public string Data { get; set; }

            public Message(string command, string subcommand, string data)
            {
                this.Command = command;
                this.Subcommand = subcommand;
                this.Data = data;
            }
        }
    }

    
}

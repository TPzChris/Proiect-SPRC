using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Connection;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    class Program
    {
        private static int server_port = 11346;
        public static List<string> selectedUsers = new List<string>();
        private static Dictionary<string, User> userlist = new Dictionary<string, User>();
        private static TCPConnection con = new TCPConnection();

        static void Main(string[] args)
        {
            con.reserve(server_port);
            con.OnReceiveCompleted += con_OnReceiveCompleted;
            con.OnExceptionRaised += con_OnExceptionRaised;
            
            Console.WriteLine("waiting connection from client.");
            System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
        }

        static void con_OnExceptionRaised(object sender, ExceptionRaiseEventArgs args)
        {
            if (sender is Socket)
            {
                try
                {
                    Socket sock = sender as Socket;
                    IPEndPoint iep = (sock.RemoteEndPoint as IPEndPoint);
                    string clientAddr = iep.Address.ToString() + iep.Port;

                    if (userlist.ContainsKey(clientAddr))
                    {
                        string uname = userlist[clientAddr].Username;
                        Console.WriteLine(uname + " lost connection.");

                        con.send(Commands.CreateMessage(Commands.Disconnect, Commands.None, uname));
                        con.close(sock);
                        userlist.Remove(clientAddr);
                    }
                    else
                        Console.WriteLine(clientAddr + " lost connection."); // unknown username
                }
                catch (ObjectDisposedException e) { }
            }
            else
            {
                if (!(sender.GetType() == typeof(Socket)))
                {
                    Console.WriteLine("exception source : " + args.raisedException.Source);
                    Console.WriteLine("exception raised : " + args.raisedException.Message);
                    Console.WriteLine("exception detail : " + args.raisedException.InnerException);
                }
            }
        }

        static void con_OnReceiveCompleted(object sender, ReceiveCompletedEventArgs rdArgs)
        {
            byte[] recData = rdArgs.data;
            //recData.ToList().ForEach(v => Console.WriteLine("V = {0}\n", v.ToString()));
            IPEndPoint iep = (rdArgs.remoteSock.RemoteEndPoint as IPEndPoint);
            string clientAddr = iep.Address.ToString() + iep.Port;

            if (!userlist.ContainsKey(clientAddr)) {
                userlist[clientAddr] = new User();
            }

            User user = userlist[clientAddr];
            string text = Encoding.Unicode.GetString(recData);

            if (user.IncompleteMessage != null)
            {
                text = user.IncompleteMessage + text;
            }

            Console.WriteLine(text + "\r\n");


            string[] messages = text.Split(new string[] { Commands.EndMessageDelim }, StringSplitOptions.RemoveEmptyEntries);

            if (messages.Length > 0)
            {
                //verifies if last message is complete (correction = 0)
                //if not (correction = 1) it will be stored for further use
                int correction = (text.EndsWith(Commands.EndMessageDelim) ? 0 : 1);
                if (correction == 1)
                {
                    user.IncompleteMessage = messages[messages.Length - 1];
                }
                else
                {
                    user.IncompleteMessage = null;
                }

                for (int i = 0; i < messages.Length - correction; i++)
                {
                    Commands.Message message = Commands.DecodeMessage(messages[i]);

                    switch (message.Command)
                    {
                        case Commands.Logout:
                            string uname = userlist[clientAddr].Username;
                            Console.WriteLine(uname + " logout successfully.");
                            
                            con.close(rdArgs.remoteSock);
                            userlist.Remove(clientAddr);

                            con.send(Commands.CreateMessage(Commands.UserList, Commands.Remove, uname));
                            break;

                        case Commands.ValidateUsername:
                            if (message.Subcommand == Commands.Request)
                            {
                                bool usernameExists = false;

                                foreach (var u in userlist)
                                {
                                    if (u.Value.Status != User.StatusType.UsernameInvalid && u.Value.Username == message.Data)
                                    {
                                        usernameExists = true;
                                        break;
                                    }
                                }

                                if (!usernameExists)
                                {
                                    user.Username = message.Data;
                                    user.Status = User.StatusType.Connecting;

                                    con.sendBySpecificSocket(Commands.CreateMessage(Commands.ValidateUsername, Commands.Accept, message.Data), rdArgs.remoteSock);
                                    break;
                                }
                            }

                            con.sendBySpecificSocket(Commands.CreateMessage(Commands.ValidateUsername, Commands.Deny, null), rdArgs.remoteSock);
                            user.Status = User.StatusType.UsernameInvalid;
                            break;

                        case Commands.Connect:
                            if(user.Status != User.StatusType.Connecting)
                            {
                                con.sendBySpecificSocket(Commands.CreateMessage(Commands.InvalidRequest, Commands.None, "Invalid request for current state."), rdArgs.remoteSock);
                            }

                            if (message.Subcommand == Commands.Request)
                            {
                                user.Status = User.StatusType.Connected;
                                user.socket = rdArgs.remoteSock;
                                con.sendBySpecificSocket(Commands.CreateMessage(Commands.Connect, Commands.Accept, null), rdArgs.remoteSock);
                            }
                            break;

                        case Commands.UserList:
                            if (user.Status != User.StatusType.Connected)
                            {
                                con.sendBySpecificSocket(Commands.CreateMessage(Commands.InvalidRequest, Commands.None, "Invalid request for current state."), rdArgs.remoteSock);
                            }

                            foreach (var u in userlist)
                            {
                                if (u.Value.Username != user.Username)
                                {
                                    byte[] data = Commands.CreateMessage(Commands.UserList, Commands.Add, u.Value.Username);
                                    //Console.WriteLine(Encoding.Unicode.GetString(data));
                                    con.sendBySpecificSocket(data, rdArgs.remoteSock);
                                }
                                userlist.ToList().ForEach(b => Console.WriteLine(b.Key));
                            }

                            con.send(Commands.CreateMessage(Commands.UserList, Commands.Add, user.Username));

                            Console.WriteLine(user.Username + " has joined this conversation.");
                            break;

                        case Commands.PublicMessage:
                            if (user.Status != User.StatusType.Connected)
                            {
                                con.sendBySpecificSocket(Commands.CreateMessage(Commands.InvalidRequest, Commands.None, "Invalid request for current state."), rdArgs.remoteSock);
                            }

                            string updateMessage = userlist[clientAddr].Username + " says : " + message.Data;
                            Console.WriteLine(updateMessage);
                            con.send(Commands.CreateMessage(Commands.PublicMessage, Commands.None, updateMessage));
                            break;

                        case Commands.PrivateMessage:
                            if (user.Status != User.StatusType.Connected)
                            {
                                con.sendBySpecificSocket(Commands.CreatePrivateMessage(Commands.InvalidRequest, Commands.None, "Invalid request for current state.", selectedUsers), rdArgs.remoteSock);
                            }
                            
                            string updateMessagePriv = "(Private) " + userlist[clientAddr].Username + " says : " + message.Data;
                            //Console.WriteLine(updateMessagePriv);
                            //string receivers = "user01";
                            //selectedUsers.ToList().ForEach(x => receivers += x + "/");
                            //List<string> s = new List<string>();
                            //s.Add("user01");
                            //con.send(Commands.CreatePrivateMessage(Commands.PrivateMessage, receivers, updateMessagePriv, s));
                            /*
                            //Socket sk = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                            IPEndPoint ipep = null;
                            //userlist.ToList().Where(x => x.Value.Username == "user01").ToList().ForEach(y => sk.Bind(ipep = new IPEndPoint(IPAddress.Parse(y.Key), 32000))); 
                            IPAddress ipaddr;
                            foreach(KeyValuePair<string, User> j in userlist)
                            {
                                if(j.Value.Username == "user01")
                                {
                                    IPAddress.TryParse(j.Key, out ipaddr);
                                    Console.WriteLine("j.ip = {0},\n j.username = {1}", j.Key, j.Value.Username);
                                    ipep = new IPEndPoint(ipaddr, 32000);
                                    Console.WriteLine("ipep {0}:{1}\n", ipep.Address, ipep.Port);
                                    //sk.Bind(ipep);
                                    break;
                                }
                            }
                             */


                            List<User> selUsers = new List<User>();
                            userlist.ToList().Where(u => message.Subcommand.Contains(u.Value.Username)).ToList().ForEach(v => selUsers.Add(v.Value));

                            selUsers.ForEach(l => con.sendBySpecificSocket(Commands.CreateMessage(Commands.PrivateMessage, Commands.UserList, updateMessagePriv), l.socket));
                            /*
                            foreach(User us in userS)
                            {
                                con.sendBySpecificSocket(Commands.CreateMessage(Commands.PrivateMessage, Commands.None, updateMessagePriv), us.socket);
                            }
                            */
                            //Console.WriteLine(++ii);
                            //con.sendBySpecificSocket(Commands.CreatePrivateMessage(Commands.PrivateMessage, receivers, updateMessagePriv, s), sk);
                            break;

                        case Commands.MalformedCommand:
                            con.sendBySpecificSocket(Commands.CreateMessage(Commands.MalformedCommand, Commands.None, null), rdArgs.remoteSock);
                            break;

                        default:
                            con.sendBySpecificSocket(Commands.CreateMessage(Commands.InvalidRequest, Commands.None, "Unknown command."), rdArgs.remoteSock);
                            break;
                    }
                }
            }
        }


        


    }
}

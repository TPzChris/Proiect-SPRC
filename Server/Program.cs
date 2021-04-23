using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Connection;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Timers;

namespace Server
{
    class Program
    {
        private static int server_port = 11346;
        public static List<string> selectedUsers = new List<string>();
        private static Dictionary<string, User> userlist = new Dictionary<string, User>();
        private static TCPConnection con = new TCPConnection();
        private static Timer t = new Timer();
        private static int seconds, minutes = 0;
        private static bool flag = false;

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

                        case Commands.UserCount:
                            if (user.Status != User.StatusType.Connected)
                            {
                                con.sendBySpecificSocket(Commands.CreateMessage(Commands.InvalidRequest, Commands.None, "Invalid request for current state."), rdArgs.remoteSock);
                            }

                            string userCountMessage = userlist.Count.ToString();
                            Console.WriteLine("userCountMessage = " + userCountMessage);
                            con.send(Commands.CreateMessage(Commands.UserCount, Commands.None, userCountMessage));
                            break;

                        case Commands.FirstIsReady:
                            if (user.Status != User.StatusType.Connected)
                            {
                                con.sendBySpecificSocket(Commands.CreateMessage(Commands.InvalidRequest, Commands.None, "Invalid request for current state."), rdArgs.remoteSock);
                            }

                            con.send(Commands.CreateMessage(Commands.FirstIsReady, Commands.None, message.Data));
                            break;

                        case Commands.SecondIsReady:
                            if (user.Status != User.StatusType.Connected)
                            {
                                con.sendBySpecificSocket(Commands.CreateMessage(Commands.InvalidRequest, Commands.None, "Invalid request for current state."), rdArgs.remoteSock);
                            }

                            con.send(Commands.CreateMessage(Commands.SecondIsReady, Commands.None, message.Data));
                            break;

                        case Commands.StartGame:
                            if (user.Status != User.StatusType.Connected)
                            {
                                con.sendBySpecificSocket(Commands.CreateMessage(Commands.InvalidRequest, Commands.None, "Invalid request for current state."), rdArgs.remoteSock);
                            }

                            List<int> numbers = new List<int>();
                            Sudoku.generateFirstRow(numbers);
                            Dictionary<int, List<int>> d = new Dictionary<int, List<int>>();
                            d.Add(0, numbers);
                            d.Add(1, Sudoku.shiftLeft(numbers, 3));
                            d.Add(2, Sudoku.shiftLeft(d[1], 3));
                            d.Add(3, Sudoku.shiftLeft(d[2], 1));
                            d.Add(4, Sudoku.shiftLeft(d[3], 3));
                            d.Add(5, Sudoku.shiftLeft(d[4], 3));
                            d.Add(6, Sudoku.shiftLeft(d[5], 1));
                            d.Add(7, Sudoku.shiftLeft(d[6], 3));
                            d.Add(8, Sudoku.shiftLeft(d[7], 3));

                            String generatedSudoku = "";
                            Random a = new Random();
                            foreach (KeyValuePair<int, List<int>> k in d)
                            {
                                for(int l = 0; l < 6; l++)
                                {
                                    int r = a.Next(k.Value.Count);
                                    k.Value[r] = 0;
                                }
                                generatedSudoku += string.Join(",", k.Value.ToArray()) + ",";
                            }

                            con.send(Commands.CreateMessage(Commands.StartGame, Commands.None, generatedSudoku));
                            break;

                        case Commands.Grid:
                            if (user.Status != User.StatusType.Connected)
                            {
                                con.sendBySpecificSocket(Commands.CreateMessage(Commands.InvalidRequest, Commands.None, "Invalid request for current state."), rdArgs.remoteSock);
                            }

                            con.send(Commands.CreateMessage(Commands.Grid, message.Subcommand, message.Data));
                            break;

                        case Commands.Label:
                            if (user.Status != User.StatusType.Connected)
                            {
                                con.sendBySpecificSocket(Commands.CreateMessage(Commands.InvalidRequest, Commands.None, "Invalid request for current state."), rdArgs.remoteSock);
                            }

                            con.send(Commands.CreateMessage(Commands.Label, message.Subcommand, message.Data));
                            break;

                        case Commands.Timer:
                            if (user.Status != User.StatusType.Connected)
                            {
                                con.sendBySpecificSocket(Commands.CreateMessage(Commands.InvalidRequest, Commands.None, "Invalid request for current state."), rdArgs.remoteSock);
                            }

                            seconds = 0; 
                            minutes = 0;
                            if (!flag)
                            {
                                t.Elapsed += new ElapsedEventHandler(OnTimedEvent);
                                t.Interval = 1000;
                                t.Enabled = true;
                                flag = true;
                            }

                            break;

                        case Commands.FirstSubmit:
                            if (user.Status != User.StatusType.Connected)
                            {
                                con.sendBySpecificSocket(Commands.CreateMessage(Commands.InvalidRequest, Commands.None, "Invalid request for current state."), rdArgs.remoteSock);
                            }

                            con.send(Commands.CreateMessage(Commands.FirstSubmit, Commands.None, message.Data));
                            break;

                        case Commands.SecondSubmit:
                            if (user.Status != User.StatusType.Connected)
                            {
                                con.sendBySpecificSocket(Commands.CreateMessage(Commands.InvalidRequest, Commands.None, "Invalid request for current state."), rdArgs.remoteSock);
                            }

                            con.send(Commands.CreateMessage(Commands.SecondSubmit, Commands.None, message.Data));
                            break;

                        case Commands.CheckGame:
                            if (user.Status != User.StatusType.Connected)
                            {
                                con.sendBySpecificSocket(Commands.CreateMessage(Commands.InvalidRequest, Commands.None, "Invalid request for current state."), rdArgs.remoteSock);
                            }

                            con.send(Commands.CreateMessage(Commands.CheckGame, Commands.None, message.Data));
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


                            List<User> selUsers = new List<User>();
                            userlist.ToList().Where(u => message.Subcommand.Contains(u.Value.Username)).ToList().ForEach(v => selUsers.Add(v.Value));

                            selUsers.ForEach(l => con.sendBySpecificSocket(Commands.CreateMessage(Commands.PrivateMessage, Commands.UserList, updateMessagePriv), l.socket));

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

        private static void OnTimedEvent(object sender, ElapsedEventArgs e)
        {
            if(++seconds == 60)
            {
                minutes++;
                seconds = 0;
            }
            string time = string.Format("{0}:{1}", minutes.ToString("00"), seconds.ToString("00"));
            con.send(Commands.CreateMessage(Commands.Timer, Commands.None, time));
        }
    }
}

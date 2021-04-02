using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace Connection
{
    public class TCPConnection : IConnection
    {
        private const int BUFFER = 8192;
        private Socket localSocket = null;
        private List<Socket> clientList = null;

        public bool connect(IPEndPoint remoteEndpoint)
        {
            try
            {
                localSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                ConnectionObject co = new ConnectionObject { 
                    workingBytes = new byte[BUFFER],
                    clientSocket = localSocket
                };

                localSocket.BeginConnect(remoteEndpoint, new AsyncCallback(OnConnect), co);
                Console.WriteLine("localSocket: {0}", localSocket.LocalEndPoint);
                return true;
            }
            catch (Exception exc)
            {
                removeUnusedSocket();
                ExceptionRaised(this, new ExceptionRaiseEventArgs(exc));
                return false;
            }
        }

        public EndPoint getRemoteEndPoint()
        {
            return localSocket.RemoteEndPoint;
        }

        public EndPoint getLocalEndPoint()
        {
            return localSocket.LocalEndPoint;
        }

        public bool reserve(int port)
        {
            try
            {
                clientList = new List<Socket>();
                localSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint localEndpoint = new IPEndPoint(IPAddress.Any, port);

                ConnectionObject co = new ConnectionObject {
                    workingBytes = new byte[BUFFER]
                };

                localSocket.Bind(localEndpoint);
                localSocket.Listen(100);
                localSocket.BeginAccept(new AsyncCallback(OnAccept), co);

                return true;
            }
            catch (Exception exc)
            {
                removeUnusedSocket();
                ExceptionRaised(this, new ExceptionRaiseEventArgs(exc));
                return false;
            }
        }

        public bool send(byte [] data)
        {
            try
            {
                if (localSocket == null)
                    return false;
                else
                {
                    ConnectionObject co = new ConnectionObject{
                        workingBytes = data,
                        clientSocket = localSocket
                    };

                    if (clientList == null)
                        localSocket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(OnSend), co);
                    else
                    {
                        foreach (var client in clientList)
                        {
                            co = new ConnectionObject
                            {
                                workingBytes = data,
                                clientSocket = client
                            };
                            client.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(OnSend), co);
                        }
                    }
                }

                return true;
            }
            catch (Exception exc)
            {
                removeUnusedSocket();
                ExceptionRaised(this, new ExceptionRaiseEventArgs(exc));
                return false;
            }
        }

        public bool sendPrivate(byte[] data)
        {
            try
            {
                if (localSocket == null)
                    return false;
                else
                {
                    ConnectionObject co = new ConnectionObject
                    {
                        workingBytes = data,
                        clientSocket = localSocket
                    };

                    if (clientList == null)
                        localSocket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(OnSend), co);
                    else
                    {
                        foreach (var client in clientList)
                        {
                            co = new ConnectionObject
                            {
                                workingBytes = data,
                                clientSocket = client
                            };
                            client.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(OnSend), co);
                        }
                    }
                }

                return true;
            }
            catch (Exception exc)
            {
                removeUnusedSocket();
                ExceptionRaised(this, new ExceptionRaiseEventArgs(exc));
                return false;
            }
        }

        public bool sendBySpecificSocket(byte[] data, Socket sockAddr)
        {
            try
            {
                ConnectionObject co = new ConnectionObject
                {
                    workingBytes = data,
                    clientSocket = sockAddr
                };

                sockAddr.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(OnSend), co);

                return true;
            }
            catch (Exception exc)
            {
                removeUnusedSocket();
                ExceptionRaised(this, new ExceptionRaiseEventArgs(exc));
                return false;
            }
        }

        public bool close()
        {
            try
            {
                localSocket.Close();

                return false;
            }
            catch (Exception exc)
            {
                removeUnusedSocket();
                ExceptionRaised(this, new ExceptionRaiseEventArgs(exc));
                return false;
            }
        }

        public bool close(Socket sock)
        {
            try
            {
                sock.Close(100);
                removeUnusedSocket();
                return false;
            }
            catch (Exception exc)
            {
                ExceptionRaised(this, new ExceptionRaiseEventArgs(exc));
                return false;
            }
        }

        // tcp async call back
        private void OnAccept(IAsyncResult ar)
        {
            try
            {
                if (ar.IsCompleted)
                {
                    ConnectionObject recObj = ar.AsyncState as ConnectionObject;
                    recObj.clientSocket = localSocket.EndAccept(ar);
                    localSocket.BeginAccept(new AsyncCallback(OnAccept), recObj);

                    ConnectionObject co = new ConnectionObject {
                        workingBytes = new byte[BUFFER],
                        clientSocket = recObj.clientSocket
                    };

                    clientList.Add(recObj.clientSocket);
                    recObj.clientSocket.BeginReceive(
                        co.workingBytes, 
                        0, 
                        co.workingBytes.Length, 
                        SocketFlags.None, new AsyncCallback(OnReceive), 
                        co);
                }
            }
            catch (Exception exc)
            {
                removeUnusedSocket();
                ExceptionRaised(this, new ExceptionRaiseEventArgs(exc));
            }
        }

        private void OnSend(IAsyncResult ar)
        {
            try
            {
                if (ar.IsCompleted)
                {
                    var sendObj = ar.AsyncState as ConnectionObject;
                    sendObj.clientSocket.EndSend(ar);
                }
            }
            catch (Exception exc)
            {
                removeUnusedSocket();
                ExceptionRaised(this, new ExceptionRaiseEventArgs(exc));
            }
        }

        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                if (ar.IsCompleted)
                {
                    ConnectionObject recObj = ar.AsyncState as ConnectionObject;

                    Socket workingSock = recObj.clientSocket;

                    int receivedBytes = workingSock.EndReceive(ar);
                    byte[] recBytes = recObj.workingBytes.Take(receivedBytes).ToArray();

                    ReceiveCompleted(this, new ReceiveCompletedEventArgs(recBytes, workingSock));

                    ConnectionObject co = new ConnectionObject
                    {
                        workingBytes = new byte[BUFFER],
                        clientSocket = workingSock
                    };

                    workingSock.BeginReceive(co.workingBytes, 0, co.workingBytes.Length, SocketFlags.None, new AsyncCallback(OnReceive), co);
                }
            }
            catch (Exception exc)
            {
                removeUnusedSocket();
                ConnectionObject recObj = ar.AsyncState as ConnectionObject;
                if(recObj.clientSocket == default(Socket))
                    ExceptionRaised(this, new ExceptionRaiseEventArgs(exc));
                else
                    ExceptionRaised(recObj.clientSocket, new ExceptionRaiseEventArgs(exc));
            }
        }

        private void OnConnect(IAsyncResult ar)
        {
            try
            {
                ConnectionObject recObj = ar.AsyncState as ConnectionObject;
                localSocket.EndConnect(ar);

                ConnectionObject co = new ConnectionObject
                {
                    workingBytes = new byte[BUFFER],
                    clientSocket = localSocket
                };

                ConnectCompleted(this, null);
                localSocket.BeginReceive(co.workingBytes, 0, co.workingBytes.Length, SocketFlags.None, new AsyncCallback(OnReceive), co);
            }
            catch (Exception exc)
            {
                removeUnusedSocket();
                ExceptionRaised(this, new ExceptionRaiseEventArgs(exc));
            }
        }

        private void removeUnusedSocket()
        {
            if (clientList != null)
            {
                foreach (var client in clientList)
                {
                    if (!client.Connected)
                    {
                        clientList.Remove(client);
                        break;
                    }
                }
            }
        }




        public delegate void ConnectCompletedHandler(object sender, EventArgs args);
        public event ConnectCompletedHandler OnConnectCompleted;
        private void ConnectCompleted(object sender, EventArgs args)
        {
            if (OnConnectCompleted != null)
            {
                OnConnectCompleted(sender, args);
            }
        }

        public delegate void ReceiveCompletedHandler(object sender, ReceiveCompletedEventArgs args);
        public event ReceiveCompletedHandler OnReceiveCompleted;
        private void ReceiveCompleted(object sender, ReceiveCompletedEventArgs args)
        {
            if (OnReceiveCompleted != null)
            {
                OnReceiveCompleted(sender, args);
            }
        }

        public delegate void ExceptionRaisedHandler(object sender, ExceptionRaiseEventArgs args);
        public event ExceptionRaisedHandler OnExceptionRaised;
        private void ExceptionRaised(object sender, ExceptionRaiseEventArgs args)
        {
            if (OnExceptionRaised != null)
            {
                OnExceptionRaised(sender, args);
            }
        }
    }
}

using System;
using System.Net;
using System.Text;
using System.Windows.Forms;
using Connection;
using Server;

namespace Client
{
    public partial class RequestName : Form
    {
        private TCPConnection con;
        private bool connected;
        private int server_port = 11346;

        public RequestName()
        {
            InitializeComponent();
            con = new TCPConnection();
            con.OnConnectCompleted += con_OnConnectCompleted;
            con.OnExceptionRaised += con_OnExceptionRaised;

            connected = false;
        }

        private void submitBtn_Click(object sender, EventArgs e)
        {
            string uname = username.Text;
            string serverip = server.Text;
            IPAddress servAddr;

            if (uname.Trim() == "")
                MessageBox.Show("Please fill your name first.");
            else if (!IPAddress.TryParse(serverip, out servAddr))
            {
                MessageBox.Show("Invalid server address.");
            }
            else
            {
                if (connected)
                {
                    string ip = con.getLocalEndPoint().ToString();
                    con.send(Commands.CreateMessage(Commands.ValidateUsername, Commands.Request, username.Text));
                    con.send(Commands.CreateMessage(Commands.EndPoint, Commands.Request, ip));
                }
                else
                {
                    con.connect(new IPEndPoint(servAddr, server_port));
                    con.send(Commands.CreateMessage(Commands.ValidateUsername, Commands.Request, username.Text));
                }
            }
        }

        delegate void FormFunctionCall();
        public void OpenChatBox()
        {
            ChatBox cb = new ChatBox(con);
            cb.Text = "ChatBox - " + username.Text;
            cb.Show();
            Hide();
        }

        private delegate void ReceiveFunctionCall(string text);
        private string incompleteMessage = null;
        private void ReceieveMessage(string text)
        {

            if (incompleteMessage != null)
            {
                text = incompleteMessage + text;
            }

            //chatField.Text += text + "\r\n";

            //chatField.SelectionStart = chatField.TextLength;
            //chatField.ScrollToCaret();

            string[] messages = text.Split(new string[] { Commands.EndMessageDelim }, StringSplitOptions.RemoveEmptyEntries);

            if (messages.Length > 0)
            {
                //verifies if last message is complete (correction = 0)
                //if not (correction = 1) it will be stored for further use
                int correction = (text.EndsWith(Commands.EndMessageDelim) ? 0 : 1);
                if (correction == 1)
                {
                    incompleteMessage = messages[messages.Length - 1];
                }
                else
                {
                    incompleteMessage = null;
                }

                Boolean countOk = false;
                Boolean usernameOk = false;

                for (int i = 0; i < messages.Length - correction; i++)
                {
                    Commands.Message message = Commands.DecodeMessage(messages[i]);
                    
                    switch(message.Command)
                    {
                        case Commands.ValidateUsername:
                            if(message.Subcommand == Commands.Accept)
                            {
                                //con.send(Commands.CreateMessage(Commands.Connect, Commands.Request, null));
                                usernameOk = true;

                                con.send(Commands.CreateMessage(Commands.UserCount, Commands.None, null));
                            }
                            else
                            {
                                MessageBox.Show("Username unavailable.");
                            }
                            break;

                        case Commands.Connect:
                            con.OnReceiveCompleted -= con_OnReceiveCompleted;
                            BeginInvoke(new FormFunctionCall(OpenChatBox));
                            break;

                        case Commands.UserCount:
                            int userCount = Int32.Parse(message.Data);
                            if (userCount < 3)
                            {
                                con.send(Commands.CreateMessage(Commands.Connect, Commands.Request, null));
                            }
                            else
                            {
                                MessageBox.Show("The room is full.");
                            }
                            break;

                    }
                    

                }
                
            }
        }

        void con_OnExceptionRaised(object sender, ExceptionRaiseEventArgs args)
        {
            Exception exc = args.raisedException;
            MessageBox.Show(exc.Message);
        }

        void con_OnConnectCompleted(object sender, EventArgs args)
        {
            connected = true;
            con.OnReceiveCompleted += con_OnReceiveCompleted;
        }

        void con_OnReceiveCompleted(object sender, ReceiveCompletedEventArgs args)
        {
            string text = Encoding.Unicode.GetString(args.data);
            BeginInvoke(new ReceiveFunctionCall(ReceieveMessage), text);
        }

        private void username_TextChanged(object sender, EventArgs e)
        {

        }
    }
}

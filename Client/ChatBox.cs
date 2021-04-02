﻿using Connection;
using Server;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Client
{
    public partial class ChatBox : Form
    {
        private TCPConnection con;
        private List<String> selectedUsers = new List<String>();
        private bool isPrivate = false;
        private Dictionary<int, string> oldValues = new Dictionary<int, string>();

        public ChatBox(TCPConnection con)
        {
            InitializeComponent();
            this.con = con;
            con.OnReceiveCompleted += con_OnReceiveCompleted;
            con.OnExceptionRaised += con_OnExceptionRaised;
            for (int h = 0; h < 81; h++)
            {
                oldValues.Add(h, "");
            }
            KeyPreview = true;
            generateCells();
            ValidateTextBoxes();
        }

        void con_OnExceptionRaised(object sender, ExceptionRaiseEventArgs args)
        {
            Application.Exit();
        }

        public ChatBox()
        {
            InitializeComponent();

        }

        private void ChatBox_Load(object sender, EventArgs e)
        {
            con.send(Commands.CreateMessage(Commands.UserList, Commands.Request, null));
        }

        private delegate void ReceiveFunctionCall(string text);
        private string incompleteMessage = null;
        private void ReceieveMessage(string text)
        {

            if (incompleteMessage != null)
            {
                text = incompleteMessage + text;
            }

            chatField.Text += text + "\r\n";

            chatField.SelectionStart = chatField.TextLength;
            chatField.ScrollToCaret();

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

                for (int i = 0; i < messages.Length - correction; i++)
                {
                    Commands.Message message = Commands.DecodeMessage(messages[i]);

                    switch (message.Command)
                    {
                        case Commands.UserList:
                            switch (message.Subcommand)
                            {
                                case Commands.Add:
                                    userlist.Items.Add(message.Data);
                                    break;
                                case Commands.Remove:
                                    userlist.Items.Remove(message.Data);
                                    chatField.Text += message.Data + " has logout.\r\n";
                                    break;
                            }
                            break;

                        case Commands.Disconnect:
                            userlist.Items.Remove(message.Data);
                            chatField.Text += message.Data + " lost connection.\r\n";
                            break;

                        case Commands.PublicMessage:
                            chatField.Text += message.Data + "\r\n";

                            chatField.SelectionStart = chatField.TextLength;
                            chatField.ScrollToCaret();
                            break;

                        case Commands.PrivateMessage:
                            chatField.Text += "(Private) " + message.Data + "\r\n";

                            chatField.SelectionStart = chatField.TextLength;
                            chatField.ScrollToCaret();
                            break;
                    }
                }
            }
        }

        void con_OnReceiveCompleted(object sender, ReceiveCompletedEventArgs args)
        {
            string text = Encoding.Unicode.GetString(args.data);
            this.BeginInvoke(new ReceiveFunctionCall(ReceieveMessage), text);
        }

        void sendMessage()
        {
            byte[] data;

            if (isPrivate)
            {
                string receivers = "";
                if (selectedUsers.Count > 1)
                {
                    selectedUsers.ToList().ForEach(x => receivers += x + "/");
                }
                else
                {
                    receivers = selectedUsers[0];
                }
                data = Commands.CreatePrivateMessage(Commands.PrivateMessage, receivers, txtChat.Text, selectedUsers);
                MessageBox.Show("Local: " + con.getLocalEndPoint().ToString());
                MessageBox.Show("Remote: " + con.getRemoteEndPoint().ToString());
                con.sendPrivate(data);
            }
            else
            {
                data = Commands.CreateMessage(Commands.PublicMessage, Commands.None, txtChat.Text);
                con.send(data);
            }


            txtChat.Text = "";
            sendBtn.Enabled = false;
        }

        private void sendBtn_Click(object sender, EventArgs e)
        {
            sendMessage();
        }

        private void txtChat_KeyUp(object sender, KeyEventArgs e)
        {
            if (txtChat.Text == "" && sendBtn.Enabled)
                sendBtn.Enabled = false;
            else if (txtChat.Text != "" && !sendBtn.Enabled)
                sendBtn.Enabled = true;
        }

        private void ChatBox_FormClosing(object sender, FormClosingEventArgs e)
        {
            con.send(Commands.CreateMessage(Commands.Logout, Commands.None, null));
            //con.close();
            Application.Exit();
        }

        private void txtChat_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtChat_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyData == Keys.Enter)
                sendMessage();
        }

        private void chatField_TextChanged(object sender, EventArgs e)
        {

        }

        private void userlist_SelectedIndexChanged(object sender, EventArgs e)
        {
            Controls.Find("userlist", true).ToList().Clear();
            var x = sender as ListBox;
            if (x.SelectedItems.Count > 0)
            {
                isPrivate = true;
            }
            else
            {
                isPrivate = false;
            }
            foreach (string l in x.SelectedItems)
            {
                selectedUsers.Add(l);
                MessageBox.Show(l);
            }

        }

        private void userlist_MouseDown(object sender, MouseEventArgs e)
        {
            Controls.Find("userlist", true).ToList().ForEach(x => /*MessageBox.Show(x.Text)*/ x.MouseClick += Item_MouseClick);
        }

        private void Item_MouseClick(object sender, MouseEventArgs e)
        {
            var x = sender as ListBox;
            string server = "127.0.0.1";
        }



        private void Panel1_Paint(object sender, PaintEventArgs e)
        {
            Pen pen = new Pen(Color.Black);
            pen.Width = 3;
            int i, j;
            for (i = 0; i < 10; i++)
            {
                for (j = 0; j < 10; j++)
                {
                    if (i % 3 != 0)
                    {
                        e.Graphics.DrawLine(Pens.Black, i * 50, 0, i * 50, 500);
                    }
                    else
                    {
                        e.Graphics.DrawLine(pen, i * 50, 0, i * 50, 500);
                    }

                    if (j % 3 != 0)
                    {
                        e.Graphics.DrawLine(Pens.Black, 0, j * 50, 500, j * 50);
                    }
                    else
                    {
                        e.Graphics.DrawLine(pen, 0, j * 50, 500, j * 50);
                    }
                }
            }

        }

        public void ValidateTextBoxes()
        {

            var textBoxes = new List<Control>();
            Controls.Find("panel1", true).ToList().ForEach(t => t.Controls.OfType<TextBox>().ToList().ForEach(t1 => textBoxes.Add(t1)));
            foreach (TextBox t in textBoxes)
            {
                t.TextChanged += Cell_TextChanged;
            }

        }



        private void Cell_TextChanged(object sender, EventArgs e)
        {
            List<string> numbers = new List<string>() { "1", "2", "3", "4", "5", "6", "7", "8", "9", "" };
            var cell = sender as TextBox;

            //MessageBox.Show(cell.Text);
            if (!numbers.Contains(cell.Text))
            {
                //MessageBox.Show("Sunt in key pressed");
                cell.Clear();
                //MessageBox.Show(getIndexOfCell(cell));
            }
            else
            {
                if (cell.Text != "")
                {
                    oldValues[int.Parse(cell.Name)] = cell.Text;
                    checkNeighbours(cell, Color.Red);
                }
                else
                {
                    fixNeighbours(cell);
                }
            }

        }


        private void generateCells()
        {
            TextBox[] cells = new TextBox[100];
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    var txt = new TextBox();
                    txt.Size = new System.Drawing.Size(20, 20);

                    Controls.Find("panel1", true).ToList().ForEach(x => x.Controls.Add(txt));
                    cells[i + 10 * j] = txt;
                    txt.Name = (i + 10 * j).ToString();
                    txt.Text = "";
                    txt.Location = new Point(15 + i * 50, 15 + j * 50);
                    txt.BackColor = Color.Gainsboro;
                    txt.BorderStyle = 0;
                    txt.Visible = true;
                }
            }
        }



        private void checkNeighbours(TextBox t, Color co)
        {
            int indexT = int.Parse(t.Name);

            List<Control> textBoxes = new List<Control>();
            Controls.Find("panel1", true).ToList().ForEach(t1 => t1.Controls.OfType<TextBox>().ToList().ForEach(t2 => textBoxes.Add(t2)));

            int x = 0;
            foreach (TextBox c in textBoxes)
            {
                if (((Int32.Parse(c.Name) % 10 == indexT % 10)
                        || (Int32.Parse(c.Name) / 10 == indexT / 10)
                        || ("" + (Int32.Parse(c.Name) % 10 / 3) + (Int32.Parse(c.Name) / 30) == ("" + indexT % 10 / 3 + indexT / 30)))
                        && c.Text == t.Text)
                {
                    if (x++ == 1)
                    {
                        //MessageBox.Show("KO");
                        foreach (TextBox c1 in textBoxes)
                        {
                            if (((Int32.Parse(c1.Name) % 10 == indexT % 10)
                                    || (Int32.Parse(c1.Name) / 10 == indexT / 10)
                                    || ("" + (Int32.Parse(c1.Name) % 10 / 3) + (Int32.Parse(c1.Name) / 30) == ("" + indexT % 10 / 3 + indexT / 30)))
                                    && c1.Text == t.Text)
                            {
                                c1.BackColor = co;
                            }
                        }
                    }

                }
            }

        }

        private void fixNeighbours(TextBox t)
        {
            int indexT = int.Parse(t.Name);
            string textT = oldValues[int.Parse(t.Name)];

            List<Control> textBoxes = new List<Control>();
            Controls.Find("panel1", true).ToList().ForEach(t1 => t1.Controls.OfType<TextBox>().ToList().ForEach(t2 => textBoxes.Add(t2)));

            foreach (TextBox c in textBoxes)
            {
                if (((Int32.Parse(c.Name) % 10 == indexT % 10)
                        || (Int32.Parse(c.Name) / 10 == indexT / 10)
                        || ("" + (Int32.Parse(c.Name) % 10 / 3) + (Int32.Parse(c.Name) / 30) == ("" + indexT % 10 / 3 + indexT / 30)))
                        && c.Text == textT)
                {
                    t.BackColor = Color.Gainsboro;
                    //MessageBox.Show("OK");
                    foreach (TextBox c1 in textBoxes)
                    {
                        if (((Int32.Parse(c1.Name) % 10 == indexT % 10)
                                || (Int32.Parse(c1.Name) / 10 == indexT / 10)
                                || ("" + (Int32.Parse(c1.Name) % 10 / 3) + (Int32.Parse(c1.Name) / 30) == ("" + indexT % 10 / 3 + indexT / 30)))
                                && c1.Text == textT)
                        {
                            //t.BackColor = Color.Gainsboro;
                            //checkNeighbours(, Color.Gainsboro);
                            //MessageBox.Show(c1.Name);
                            foreach (TextBox c2 in textBoxes)
                            {
                                if (((Int32.Parse(c2.Name) % 10 == indexT % 10)
                                        || (Int32.Parse(c2.Name) / 10 == indexT / 10)
                                        || ("" + (Int32.Parse(c2.Name) % 10 / 3) + (Int32.Parse(c2.Name) / 30) == ("" + indexT % 10 / 3 + indexT / 30)))
                                        && c2.Text == textT)
                                {
                                    //t.BackColor = Color.Gainsboro;
                                    
                                    //MessageBox.Show(c2.Name);
                                    c2.BackColor = Color.Gainsboro;
                                    checkNeighbours(c2, Color.Red);
                                }

                            }
                            //c1.BackColor = Color.Gainsboro;
                        }

                    }

                    break;
                }
            }

        }

    }
}
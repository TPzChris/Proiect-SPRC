using Connection;
using Server;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Client
{
    public partial class ChatBox : Form
    {
        private TCPConnection con;
        private List<String> selectedUsers = new List<String>();
        private bool isPrivate = false;
        private Dictionary<int, string> oldValues = new Dictionary<int, string>();
        private bool isPencil = false;
        Dictionary<TextBox, List<Label>> tbLabels = new Dictionary<TextBox, List<Label>>();
        List<Label> activeLabels = new List<Label>();
        int userCount = 0;
        TextBox[] cells;
        List<Control> textBoxes;
        private bool start = false;
        private bool changeTextClient = false;
        private Timer timer1 = new Timer();
        private bool checkGame = false;


        public ChatBox(TCPConnection con)
        {
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            InitializeComponent();
            button2.Enabled = false;
            this.con = con;
            con.OnReceiveCompleted += con_OnReceiveCompleted;
            con.OnExceptionRaised += con_OnExceptionRaised;
            labelTimer.Width = 70;
            labelTimer.Height = 40;
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

            //chatField.Text += text + "\r\n";

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
                                    chatField.Text += message.Data + " has connected.\r\n";
                                    chatField.SelectionStart = chatField.TextLength;
                                    chatField.ScrollToCaret();
                                    break;
                                case Commands.Remove:
                                    userlist.Items.Remove(message.Data);
                                    chatField.Text += message.Data + " has logout.\r\n";
                                    break;
                            }
                            break;

                        case Commands.UserCount:
                            userCount = Int32.Parse(message.Data);
                            if (userCount > 1)
                            {
                                button2.Enabled = true;
                            }
                            else
                            {
                                button2.Enabled = false;
                            }
                            break;

                        case Commands.FirstIsReady:
                            if (message.Data != con.getLocalEndPoint().ToString())
                            {
                                DialogResult dialogResult = MessageBox.Show("Do you want to start a new game?", "New Game", MessageBoxButtons.YesNo);
                                if (dialogResult == DialogResult.Yes)
                                {
                                    con.send(Commands.CreateMessage(Commands.SecondIsReady, Commands.None, "yes"));
                                }
                                else if (dialogResult == DialogResult.No)
                                {
                                    con.send(Commands.CreateMessage(Commands.SecondIsReady, Commands.None, "no"));
                                }
                            }
                            break;

                        case Commands.SecondIsReady:
                            if (message.Data == "yes")
                            {
                                con.send(Commands.CreateMessage(Commands.StartGame, Commands.None, null));
                            }
                            break;

                        case Commands.StartGame:
                            start = false;
                            Controls.Find("panel1", true).ToList().ForEach(t => t.Controls.OfType<TextBox>().ToList().ForEach(t1 => t1.Text = ""));
                            Controls.Find("panel1", true).ToList().ForEach(t => t.Controls.OfType<TextBox>().ToList().Where(t1 => t1.Text == "").ToList().ForEach(t2 => { t2.Enabled = true; t2.Visible = true; }));
                            Controls.Find("panel1", true).ToList().ForEach(l => l.Controls.OfType<Label>().ToList().ForEach(l1 => l1.Enabled = true));
                            Controls.Find("panel1", true).ToList().ForEach(l => l.Controls.OfType<Label>().ToList().ForEach(l1 => l1.VisibleChanged += label_onVisibleChanged));
                            string[] board = message.Data.Split(',');
                            for(int el = 0; el < textBoxes.Count; el++)
                            {
                                if (board[el] != "0")
                                {
                                    //MessageBox.Show(string.Format("el = {0}; elAt = {1}", board[el], textBoxes.ElementAt(el).Name));
                                    textBoxes.ElementAt(el).Text = board[el];
                                    textBoxes.ElementAt(el).Enabled = false;
                                    textBoxes.ElementAt(el).ForeColor = Color.Blue;
                                }
                                else
                                {
                                    textBoxes.ElementAt(el).Text = "";
                                }
                                Pencil.Enabled = true;
                                Pencil.Visible = true;
                            }
                            start = true;

                            
                            timer1.Tick += new EventHandler(timer1_Tick);

                            timer1.Interval = 1000;

                            timer1.Enabled = true;

                            con.send(Commands.CreateMessage(Commands.Timer, Commands.None, null));

                            break;

                        case Commands.Timer:
                            labelTimer.Text =  message.Data;
                            break;

                        case Commands.Grid:
                            //var mStream = new MemoryStream();
                            //var binFormatter = new BinaryFormatter();

                            //Where 'objectBytes' is your byte array.
                            //mStream.Write(message.Data., 0, message.Data.Length);
                            //mStream.Position = 0;

                            //var myObject = binFormatter.Deserialize(mStream) as YourObjectType;

                            //MessageBox.Show(message.Data);
                            //List<Tuple<string, string, string>> tupleList = message.Data.ToList();
                            if (!con.getLocalEndPoint().ToString().Equals(message.Subcommand))
                            {
                                MatchCollection tbRegex = Regex.Matches(message.Data, "[_][_]");

                                //MessageBox.Show(String.Format("local = {0}; subcommand = {1}", con.getLocalEndPoint().ToString(), message.Subcommand));

                                string tb = message.Data.Substring(0, message.Data.IndexOf('_'));
                                string tbVal = "";
                                string lb = "";

                                if (tbRegex.Count == 0 && !'_'.Equals(message.Data.Last()))
                                {
                                    //MessageBox.Show(message.Data + "  " + tb);
                                    tbVal = message.Data.Substring(message.Data.IndexOf('_') + 1, 1);
                                }
                                changeTextClient = true;

                                
                                //lb = message.Data.Substring(message.Data.LastIndexOf('_') + 1);
                                //List<string> labels = lb.Split('-').ToList();
                                //Controls.Find("panel1", true).ToList().ForEach(x => x.Controls.OfType<TextBox>().ToList().Where(x1 => (x1.Name.Equals(tb)) && (!x1.Text.Equals(tbVal))).First().Text = tbVal);//.Text = tbVal);
                                //Controls.Find("panel1", true).ToList().ForEach(l => l.Controls.OfType<Label>().ToList().Where(l1 => (l1.Name.StartsWith(tb + "_")) && (labels.Contains(l1.Text))).ToList().ForEach(l2 => l2.Visible = true));
                                //Controls.Find("panel1", true).ToList().ForEach(l => l.Controls.OfType<Label>().ToList().Where(l1 => (l1.Name.StartsWith(tb + "_")) && (!labels.Contains(l1.Text))).ToList().ForEach(l2 => l2.Visible = false));

                                List<Tuple<TextBox, List<Label>>> list = tbLabels.Select(x => new Tuple<TextBox, List<Label>>(x.Key, x.Value)).ToList();

                                TextBox t = new TextBox();
                                t = list.Find(x => x.Item1.Name == tb).Item1;
                                t.Text = tbVal;
                                if (t.Text != "")
                                {
                                    oldValues[int.Parse(t.Name)] = t.Text;
                                    checkNeighbours(t, Color.Red);
                                }
                                else
                                {
                                    fixNeighbours(t);
                                }

                                //List<Label> labs = new List<Label>();
                                //labs = list.Find(x => x.Item1.Name == tb).Item2;
                                //if(labs != null)
                                //{
                                //    //labs.Where(x => labels.Contains(x.Text)).ToList().ForEach(x1 => x1.Enabled = true);
                                //    //labs.Where(x => !labels.Contains(x.Text)).ToList().ForEach(x1 => x1.Enabled = false);
                                //    //tbLabels[t] = labs;
                                //}
                                
                                //tbLabels;


                                changeTextClient = false;
                                //MessageBox.Show(string.Format("tb = {0}; tbValue = {1}; lb = {2}", tb, tbVal, lb));
                            }
                            
                            break;

                        case Commands.Label:
                            Controls.Find("panel1", true).ToList().ForEach(l => l.Controls.OfType<Label>().ToList().Where(l1 => l1.Name.Equals(message.Data)).ToList().ForEach(l2 => { l2.Enabled = Boolean.Parse(message.Subcommand); l2.Visible = Boolean.Parse(message.Subcommand); }));
                            break;

                        case Commands.FirstSubmit:
                            if (message.Data != con.getLocalEndPoint().ToString())
                            {
                                DialogResult dialogResult = MessageBox.Show("Do you want to submit?", "Submit", MessageBoxButtons.YesNo);
                                if (dialogResult == DialogResult.Yes)
                                {
                                    con.send(Commands.CreateMessage(Commands.SecondSubmit, Commands.None, "yes"));
                                }
                                else if (dialogResult == DialogResult.No)
                                {
                                    con.send(Commands.CreateMessage(Commands.SecondSubmit, Commands.None, "no"));
                                }
                            }
                            break;

                        case Commands.SecondSubmit:
                            if (message.Data == "yes")
                            {
                                Controls.Find("panel1", true).ToList().ForEach(t => checkGame = t.Controls.OfType<TextBox>().ToList().Where(x => (x.Text == "") || (x.ForeColor == Color.Red)).ToList().Any());
                                if (checkGame)
                                {
                                    con.send(Commands.CreateMessage(Commands.CheckGame, Commands.None, true.ToString()));
                                }
                                else
                                {
                                    con.send(Commands.CreateMessage(Commands.CheckGame, Commands.None, false.ToString()));
                                }
                                
                            }
                            break;

                        case Commands.CheckGame:
                            if (Boolean.Parse(message.Data))
                            {
                                MessageBox.Show("Meciul nu este gata...");
                            }
                            else
                            {
                                MessageBox.Show("Totul este corect!");
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

        private void timer2_Tick(object sender, EventArgs e)
        {
            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.labelTime.Text = DateTime.Now.ToString();
            
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
            //string server = "127.0.0.1";
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

            textBoxes = new List<Control>();
            Controls.Find("panel1", true).ToList().ForEach(t => t.Controls.OfType<TextBox>().ToList().ForEach(t1 => textBoxes.Add(t1)));
            foreach (TextBox t in textBoxes)
            {
                t.TextChanged += Cell_TextChanged;
                t.MouseEnter += TextBox_OnEnter;
                t.MouseLeave += TextBox_OnLeave;
                
            }

        }

        private void Cell_OnKeyDown(object sender, KeyEventArgs e)
        {
            var tb = sender as TextBox;
            Controls.Find("panel1", true).ToList().ForEach(t => t.Controls.OfType<Label>().ToList().Where(x => x.Name == tb.Name + "_" + tb.Text).ToList().ForEach(u => u.Visible = true));
        }

        private void TextBox_OnLeave(object sender, EventArgs e)
        {
            var textBox = sender as TextBox;
            var label = new Label();
            //Controls.Find("panel1", true).ToList().ForEach(t => t.Controls.OfType<TextBox>().ToList().Where(x => x.Name == textBox.Name).ToList().ForEach(y => y.Controls.OfType<Label>().ToList().Where(z => z.Name.EndsWith("5")).ToList().ForEach(u => MessageBox.Show(u.Name))));
            Controls.Find("panel1", true).ToList().ForEach(t => t.Controls.OfType<Label>().ToList().Where(x => x.Name == textBox.Name + "_5").ToList().ForEach(u => label = u));
            label.BringToFront();
        }

        private void TextBox_OnEnter(object sender, EventArgs e)
        {
            var textBox = sender as TextBox;
            textBox.BringToFront();
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
                tbLabels[cell] = null;
                //MessageBox.Show(getIndexOfCell(cell));
            }
            else if(numbers.Contains(cell.Text) && !changeTextClient)
            {
                if (isPencil == true)
                {
                    Label label = new Label();
                    
                    Controls.Find("panel1", true).ToList()
                        .ForEach(t => t.Controls.OfType<Label>()
                        .ToList()
                        .Where(x => x.Name == cell.Name + "_" + cell.Text)
                        .ToList().ForEach(y => { label = y; cell.Clear(); }));//Where(u => u.Visible ? MessageBox.Show("true") : {u.Visible = true; u.BringToFront(); cell.Clear(); activeLabels.Add(u); })//.  (u.Visible ? MessageBox("true") : { u.Visible = true; u.BringToFront(); cell.Clear(); activeLabels.Add(u); }) > 0));
                    if (label.Visible)
                    {
                        label.Visible = false;
                        cell.BringToFront();
                        activeLabels.Remove(label);
                    }
                    else
                    {
                        label.Visible = true;
                        label.BringToFront(); 
                        activeLabels.Add(label);
                    }
                    tbLabels[cell] = activeLabels;
                }
                else
                {
                    Controls.Find("panel1", true).ToList()
                        .ForEach(t => t.Controls.OfType<Label>()
                        .ToList()
                        .Where(x => x.Name.StartsWith(cell.Name + "_")).ToList().ForEach(y => y.Visible = false));
                    
                    tbLabels[cell] = null; 
                    
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
            if (start && numbers.Contains(cell.Text))
            {
                //TB cell
                //LB activeLabels
                string sendData = cell.Name + "_" + cell.Text + "_";
                activeLabels.ForEach(l => sendData += l.Text + "-");
                sendData = sendData.Remove(sendData.Length - 1);
                List<Tuple<string, string, List<string>>> listTuple = new List<Tuple<string, string, List<String>>>();
                Tuple<string, string, List<String>> tuple;
                foreach (KeyValuePair<TextBox, List<Label>> k in tbLabels)
                {
                    if (k.Value != null)
                    {
                        List<String> labels = new List<String>();
                        foreach (Label s in k.Value)
                        {
                            labels.Add(s.Text);
                        }
                        tuple = new Tuple<String, String, List<String>>(k.Key.Name, k.Key.Text, labels);
                    }
                    else
                    {
                        tuple = new Tuple<String, String, List<String>>(k.Key.Name, k.Key.Text, null);
                    }

                    listTuple.Add(tuple);
                }
                //string.Join(",", listTuple.ToArray() as object[])
                con.send(Commands.CreateMessage(Commands.Grid, con.getLocalEndPoint().ToString(), sendData));
            }
            

        }


        private void generateCells()
        {
            cells = new TextBox[100];

            tbLabels = new Dictionary<TextBox, List<Label>>();
            for (int y = 0; y < 89; y++)
            {
                TextBox t = new TextBox();
                t.Name = y.ToString();
                if (y % 10 != 9)
                {
                    //tbLabels.Add(t, null);
                }
            }
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
                    txt.Location = new Point(15 + j * 50, 15 + i * 50);
                    txt.ForeColor = Color.Black;
                    txt.BackColor = Color.Gainsboro;
                    txt.MaxLength = 1;
                    txt.BorderStyle = 0;
                    txt.Visible = true;
                    txt.Enabled = false;
                    
                         
                    List<Label> labelList = new List<Label>();
                    int c = 0;
                    for(int k = 0; k < 3; k++)
                    {
                        for(int l = 0; l < 3; l++)
                        {
                            var label = new Label();
                            Controls.Find("panel1", true).ToList().ForEach(x => x.Controls.Add(label));
                            label.Name = txt.Name + "_" + ++c;
                            label.Text = "" + c;
                            label.Location = new Point(j * 50 + 16 * l, i * 50 + 16 * k);
                            label.Size = new System.Drawing.Size(15, 15);
                            label.ForeColor = Color.Blue;
                            label.BackColor = Color.Gainsboro;
                            label.Visible = false;
                            label.Enabled = false;
                            label.BringToFront();

                            labelList.Add(label);

                            //MessageBox.Show(label.Location.X + " " + label.Location.Y);
                        }
                    }
                    tbLabels.Add(txt, null);
                    //tbLabels[txt] = labelList;
                }
            }
        }

        private void label_onVisibleChanged(object sender, EventArgs e)
        {
            Label send = sender as Label;
            //MessageBox.Show(string.Format("Name = {0}; Enabled = {1}", send.Name, send.Enabled));
            con.send(Commands.CreateMessage(Commands.Label, send.Visible.ToString(), send.Name));
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
                                c1.ForeColor = co;
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
                    t.ForeColor = Color.Black;
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
                                    c2.ForeColor = Color.Black;
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

        private void button1_Click(object sender, EventArgs e)
        {
            
        }

        private void Pencil_CheckedChanged(object sender, EventArgs e)
        {
            isPencil = !isPencil;
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            con.send(Commands.CreateMessage(Commands.FirstSubmit, Commands.None, con.getLocalEndPoint().ToString()));
        }

        private void userlist_CollectionChanged(object sender, EventArgs e)
        {
            
        }

        private void userlist_SizeChanged(object sender, EventArgs e)
        {
            con.send(Commands.CreateMessage(Commands.UserCount, Commands.Request, null));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            con.send(Commands.CreateMessage(Commands.FirstIsReady, Commands.Request, con.getLocalEndPoint().ToString()));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            List<Tuple<string, string, List<string>>> listTuple = new List<Tuple<string, string, List<String>>>();
            Tuple<string, string, List<String>> tuple;
            foreach (KeyValuePair<TextBox, List<Label>> k in tbLabels)
            {
                if(k.Value != null)
                {
                    List<String> labels = new List<String>();
                    foreach (Label s in k.Value)
                    {
                        labels.Add(s.Text);
                    }
                    tuple = new Tuple<String, String, List<String>>(k.Key.Name, k.Key.Text, null); //labels
                }
                else
                {
                    tuple = new Tuple<String, String, List<String>>(k.Key.Name, k.Key.Text, null);
                }

                listTuple.Add(tuple);
            }
            //Tuple<string, string, List<string>> tuple = new Tuple<String, String, List<string>>();
            //var binFormatter = new BinaryFormatter();
            //var mStream = new MemoryStream();
            //binFormatter.Serialize(mStream, listTuple);

            //This gives you the byte array.
            //mStream.ToArray();
            con.send(Commands.CreateMessage(Commands.Grid, con.getLocalEndPoint().ToString(), string.Join(",", listTuple.ToArray() as object[])));
        }

        private void labelTimer_Click(object sender, EventArgs e)
        {

        }
    }
}

namespace Client
{
    partial class ChatBox
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.userlist = new System.Windows.Forms.ListBox();
            this.chatField = new System.Windows.Forms.TextBox();
            this.txtChat = new System.Windows.Forms.TextBox();
            this.sendBtn = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.Pencil = new System.Windows.Forms.CheckBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.labelTime = new System.Windows.Forms.Label();
            this.labelTimer = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // userlist
            // 
            this.userlist.FormattingEnabled = true;
            this.userlist.Location = new System.Drawing.Point(871, 12);
            this.userlist.Name = "userlist";
            this.userlist.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.userlist.Size = new System.Drawing.Size(84, 251);
            this.userlist.TabIndex = 0;
            this.userlist.SelectedIndexChanged += new System.EventHandler(this.userlist_SelectedIndexChanged);
            this.userlist.SizeChanged += new System.EventHandler(this.userlist_SizeChanged);
            this.userlist.MouseDown += new System.Windows.Forms.MouseEventHandler(this.userlist_MouseDown);
            // 
            // chatField
            // 
            this.chatField.Location = new System.Drawing.Point(624, 12);
            this.chatField.Multiline = true;
            this.chatField.Name = "chatField";
            this.chatField.ReadOnly = true;
            this.chatField.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.chatField.Size = new System.Drawing.Size(241, 225);
            this.chatField.TabIndex = 1;
            this.chatField.TextChanged += new System.EventHandler(this.chatField_TextChanged);
            // 
            // txtChat
            // 
            this.txtChat.Location = new System.Drawing.Point(624, 243);
            this.txtChat.Name = "txtChat";
            this.txtChat.Size = new System.Drawing.Size(160, 20);
            this.txtChat.TabIndex = 2;
            this.txtChat.TextChanged += new System.EventHandler(this.txtChat_TextChanged);
            this.txtChat.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtChat_KeyDown);
            this.txtChat.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtChat_KeyUp);
            // 
            // sendBtn
            // 
            this.sendBtn.Location = new System.Drawing.Point(790, 243);
            this.sendBtn.Name = "sendBtn";
            this.sendBtn.Size = new System.Drawing.Size(75, 23);
            this.sendBtn.TabIndex = 3;
            this.sendBtn.Text = "Send";
            this.sendBtn.UseVisualStyleBackColor = true;
            this.sendBtn.Click += new System.EventHandler(this.sendBtn_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Gainsboro;
            this.panel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.panel1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(450, 450);
            this.panel1.TabIndex = 4;
            this.panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.Panel1_Paint);
            // 
            // Pencil
            // 
            this.Pencil.Appearance = System.Windows.Forms.Appearance.Button;
            this.Pencil.AutoSize = true;
            this.Pencil.Enabled = false;
            this.Pencil.Location = new System.Drawing.Point(519, 27);
            this.Pencil.Name = "Pencil";
            this.Pencil.Size = new System.Drawing.Size(46, 23);
            this.Pencil.TabIndex = 6;
            this.Pencil.Text = "Pencil";
            this.Pencil.UseVisualStyleBackColor = true;
            this.Pencil.Visible = false;
            this.Pencil.CheckedChanged += new System.EventHandler(this.Pencil_CheckedChanged);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(519, 277);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 7;
            this.button1.Text = "Submit";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(677, 374);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 8;
            this.button2.Text = "Start";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(519, 57);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(46, 23);
            this.button3.TabIndex = 9;
            this.button3.Text = "Pass";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // labelTime
            // 
            this.labelTime.AutoSize = true;
            this.labelTime.Location = new System.Drawing.Point(955, 449);
            this.labelTime.Name = "labelTime";
            this.labelTime.Size = new System.Drawing.Size(0, 13);
            this.labelTime.TabIndex = 10;
            // 
            // labelTimer
            // 
            this.labelTimer.AutoSize = true;
            this.labelTimer.Location = new System.Drawing.Point(469, 445);
            this.labelTimer.Name = "labelTimer";
            this.labelTimer.Size = new System.Drawing.Size(35, 13);
            this.labelTimer.TabIndex = 11;
            this.labelTimer.Text = "label1";
            this.labelTimer.Click += new System.EventHandler(this.labelTimer_Click);
            // 
            // ChatBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(967, 470);
            this.Controls.Add(this.labelTimer);
            this.Controls.Add(this.labelTime);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.Pencil);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.sendBtn);
            this.Controls.Add(this.txtChat);
            this.Controls.Add(this.chatField);
            this.Controls.Add(this.userlist);
            this.Name = "ChatBox";
            this.Text = "ChatBox";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ChatBox_FormClosing);
            this.Load += new System.EventHandler(this.ChatBox_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox userlist;
        private System.Windows.Forms.TextBox chatField;
        private System.Windows.Forms.TextBox txtChat;
        private System.Windows.Forms.Button sendBtn;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckBox Pencil;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Label labelTime;
        private System.Windows.Forms.Label labelTimer;
    }
}
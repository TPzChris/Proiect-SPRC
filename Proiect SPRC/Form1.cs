using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Proiect_SPRC
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            KeyPreview = true;
            generateCells();
            ValidateTextBoxes();
        }


        public void ValidateTextBoxes()
        {
            var textBoxes = new List<Control>();
            Controls.Find("panel1", true).ToList().ForEach(t => t.Controls.OfType<TextBox>().ToList().ForEach(t1 => textBoxes.Add(t1)));
            //Controls.Find("panel1", true).ToList().ForEach(t => t.);
            /*
            foreach (Control c in Controls)
            {
                foreach (Control c1 in c.Controls)
                {
                    if (c1 is TextBox)
                    {
                        textBoxes.Add(c);
                    }
                }
            }
            */
            //MessageBox.Show(textBoxes.Count.ToString());
            //MessageBox.Show(this.Controls.Count.ToString());
            //MessageBox.Show("Sunt in validate");
            foreach (TextBox t in textBoxes)
            {
                t.KeyUp += Cell_KeyUp;
            }

        }

       

        private void Cell_KeyUp(object sender, KeyEventArgs e)
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
                checkNeighbours(cell);
            }

        }


        private void generateCells()
        {
            TextBox[] cells = new TextBox[100];
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if(i == 8 && j == 8)
                    {
                        //break;
                    }
                    var txt = new TextBox();
                    txt.Size = new System.Drawing.Size(20, 20);

                    Controls.Find("panel1", true).ToList().ForEach(x => x.Controls.Add(txt));
                    cells[10 * i + j] = txt;
                    txt.Name = (i * 10 + j).ToString();
                    txt.Text = "";
                    txt.Location = new Point(15 + i * 50, 15 + j * 50);
                    txt.BackColor = Color.Gainsboro;
                    txt.BorderStyle = 0;
                    txt.Visible = true;
                }
            }
        }




        private void checkNeighbours(TextBox t)
        {
            int indexT = Int32.Parse(t.Name);

            List<Control> textBoxes = new List<Control>();
            Controls.Find("panel1", true).ToList().ForEach(t1 => t1.Controls.OfType<TextBox>().ToList().ForEach(t2 => textBoxes.Add(t2)));

            int x = 0;
            foreach(TextBox c in textBoxes)
            {
                if((Int32.Parse(c.Name) % 10 == indexT % 10 && c.Text == t.Text) || (Int32.Parse(c.Name) / 10 == indexT / 10 && c.Text == t.Text))
                {
                    if (x++ == 1)
                    {
                        MessageBox.Show("Opa");
                        foreach (TextBox c1 in textBoxes)
                        {
                            if ((Int32.Parse(c1.Name) % 10 == indexT % 10 && c1.Text == t.Text) || (Int32.Parse(c1.Name) / 10 == indexT / 10 && c1.Text == t.Text))
                            {
                                c1.BackColor = Color.Red;
                                
                            }
                        }
                    }
                    
                }
            }
        }

        private void checkNeighbours1(TextBox c, TextBox t, List<Control> textBoxes, int x)
        {
            
            if ((Int32.Parse(c.Name) % 10 == Int32.Parse(t.Name) % 10 && c.Text == t.Text) || (Int32.Parse(c.Name) / 10 == Int32.Parse(t.Name) / 10 && c.Text == t.Text))
            {
                if (x++ == 1)
                {
                    MessageBox.Show("Opa");
                    foreach (TextBox c1 in textBoxes)
                    {
                        if ((Int32.Parse(c1.Name) % 10 == Int32.Parse(t.Name) % 10 && c1.Text == t.Text) || (Int32.Parse(c1.Name) / 10 == Int32.Parse(t.Name) / 10 && c1.Text == t.Text))
                        {
                            c1.BackColor = Color.Red;

                        }
                    }
                }

            }
        }




        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            Pen pen = new Pen(Color.Black);
            pen.Width = 3;
            int i, j;
            for(i = 0; i < 10; i++)
            {
                for(j = 0; j < 10; j++)
                {
                    if (i % 3 != 0)
                    {
                        e.Graphics.DrawLine(Pens.Black, i * 50, 0, i * 50, 500);
                    }
                    else
                    {
                        e.Graphics.DrawLine(pen, i * 50, 0, i * 50, 500);
                    }    
                    
                    if(j % 3 != 0) 
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
    }
}

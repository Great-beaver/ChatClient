using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ChatClient
{

    public partial class Form1 : Form
    {
        private Ports comPort;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            

        }

        private void button1_Click(object sender, EventArgs e)
        {
           // comPort.SendTextMessage(richTextBox1.Text, 0);

          OpenFileDialog od = new OpenFileDialog();
         
          if (od.ShowDialog() == DialogResult.OK)
          {
              MessageBox.Show(od.FileName);
              comPort.SendFileTransferRequest(od.FileName, 0);
          }



            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            comPort = new Ports(textBox1.Text, textBox2.Text, 0);
        }
    }
}

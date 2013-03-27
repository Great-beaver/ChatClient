using System;
using System.Windows.Forms;

namespace ChatClient
{

    public partial class Form1 : Form
    {
        private ClientPort comPort;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            

        }

        private void button1_Click(object sender, EventArgs e)
        {
            comPort.SendTextMessage(richTextBox2.Text, Convert.ToByte(textBox4.Text));
           
        }

        private void button2_Click(object sender, EventArgs e)
        {
            comPort = new ClientPort(textBox1.Text, textBox2.Text, Convert.ToByte(textBox3.Text));
            timer1.Enabled = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog od = new OpenFileDialog();

            if (od.ShowDialog() == DialogResult.OK)
            {
                MessageBox.Show(od.FileName);
                comPort.SendFileTransferRequest(od.FileName, Convert.ToByte(textBox4.Text));
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            lock (comPort.InputMessageQueue)
            {
                if (comPort.InputMessageQueue.Count > 0)
                {
                    richTextBox1.AppendText(comPort.InputMessageQueue.Dequeue().ToString() + '\n');
                }
            }           
        }

        private void button4_Click(object sender, EventArgs e)
        {
            comPort.CancelSendingFile();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            comPort.CancelRecivingFile();
        }
    }
}

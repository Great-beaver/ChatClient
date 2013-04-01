using System;
using System.Text;
using System.Windows.Forms;
using ChatClient.Main;

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


        public delegate void ReciveMessageDelegate(string type, string text);


        void ReciveMessage(string type, string text)
        {
            if (type == "Text")
            {
                richTextBox1.AppendText(text + '\n');
            }

        }

        void ComPortMessageRecived(object sender, MessageRecivedEventArgs e)
        {

            BeginInvoke(new MethodInvoker(() => richTextBox1.AppendText(e.MessageText + '\n')));

            //ReciveMessageDelegate rr = new ReciveMessageDelegate(ReciveMessage(e.MessageText, e.MessageType);

          //  BeginInvoke(new MethodInvoker(() => richTextBox1.AppendText(Encoding.UTF8.GetString(e.Packet.Data.Content) + '\n')));

            BeginInvoke(new ReciveMessageDelegate(ReciveMessage), e.MessageText, e.MessageType);

        }

        private void button1_Click(object sender, EventArgs e)
        {
            comPort.SendTextMessage(richTextBox2.Text, Convert.ToByte(textBox4.Text));
            richTextBox2.Clear();    
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            comPort = new ClientPort(textBox1.Text, textBox2.Text, Convert.ToByte(textBox3.Text),Convert.ToInt32(textBox5.Text));
            comPort.PacketRecived += new EventHandler<MessageRecivedEventArgs>(ComPortMessageRecived);
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

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (comPort != null)
            {
                comPort.Dispose();
            }
        }
    }
}

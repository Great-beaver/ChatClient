using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ChatClient
{
    class Ports
    {
        private bool _continue = false;
        private SerialPort _comPort;
        private Thread _readThread;
        private int _clietnId ;
        private byte[] _readBufferHeader = new byte[6];
        private Crc16 _crc16 = new Crc16();

        public Ports ( string comPortName, int id)
        {
            _comPort = new SerialPort();
            _comPort.PortName = comPortName;
            _comPort.BaudRate = 9600;
            _comPort.Parity = Parity.None;
            _comPort.DataBits = 8;
            _comPort.StopBits = StopBits.One;
            _comPort.Handshake = Handshake.None;
            _comPort.ReadTimeout = 500;
            _comPort.WriteTimeout = 500;

            _clietnId = id;

            _readThread = new Thread(Read);
            _comPort.Open();
            _continue = true;
            _readThread.Start();

        }

        

        private void Read()
        {
            while (_continue)
            {
                if (_comPort.BytesToRead > 1 && _comPort.ReadByte() == 0xAA && _comPort.ReadByte() == 0x55 )
                {
                    MessageBox.Show("Ok");
                    _comPort.Read(_readBufferHeader, 0, 6);

                }

            }
        }




    }
}

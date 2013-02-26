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
        byte[] _reciveMessageLenght;

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

            _comPort.Encoding = Encoding.GetEncoding(28591);

            _clietnId = id;

            _readThread = new Thread(Read);
            _comPort.Open();
            _continue = true;
            _readThread.Start();
        }


        public void SendPacket(string message)
        {

            byte[] outPacket = new byte[8];
            byte[] packetWithOutHash = new byte[4];

            outPacket[0] = 0xAA;
            outPacket[1] = 0x55;

            Array.Copy(BitConverter.GetBytes(((short)message.Length)), 0, packetWithOutHash, 0, 2);

            packetWithOutHash[2] = 0x48;
            packetWithOutHash[3] = 0x49;

            Array.Copy(packetWithOutHash, 0, outPacket, 2, 4);

            Array.Copy(_crc16.ComputeChecksumBytes(packetWithOutHash),0,outPacket,6,2);

            // just hint to array copy

           // Array.Copy(a, 1, b, 0, 3);
           // a = source array
           // 1 = start index in source array
           // b = destination array
           // 0 = start index in destination array
           // 3 = elements to copy

            MessageBox.Show(System.Text.Encoding.UTF8.GetString(outPacket));

            MessageBox.Show(Convert.ToString(BitConverter.ToInt16(packetWithOutHash, 2)));

            

            _comPort.Write(outPacket,0,8);

        }

        private void Read()
        {
            while (_continue)
            {
                if (_comPort.BytesToRead > 1 && _comPort.ReadByte() == 0xAA && _comPort.ReadByte() == 0x55)
                {
                    if (_comPort.BytesToRead > 5)
                    {
                        MessageBox.Show("Ok");

                        byte[] messageHeaderWithoutHash = new byte[4];
                        _comPort.Read(messageHeaderWithoutHash, 0, 4);

                      //  MessageBox.Show("Calculated hash = " + Convert.ToString(_crc16.ComputeChecksum(messageHeaderWithoutHash))
                      //      + "Recived hash = " + Convert.ToString(BitConverter.ToUInt16(new byte[] { (byte)_comPort.ReadByte(), (byte)_comPort.ReadByte() }, 0)));

           

                       if (_crc16.ComputeChecksum(messageHeaderWithoutHash) == BitConverter.ToInt16(new byte[] { (byte)_comPort.ReadByte(), (byte)_comPort.ReadByte() }, 0))
                       {
                           MessageBox.Show("Hash matches!");
                       }
                

              //
              //          short lenght =
              //              BitConverter.ToInt16(new byte[] {(byte) _comPort.ReadByte(), (byte) _comPort.ReadByte()}, 0);
              //          _comPort.ReadByte();
              //          _comPort.ReadByte();
              //          MessageBox.Show(lenght.ToString());
              //          MessageBox.Show(
              //              _crc16.ComputeChecksum(new byte[] {(byte) _comPort.ReadByte(), (byte) _comPort.ReadByte()})
              //                    .ToString());
              //
              //          //_comPort.Read(_reciveMessageLenght, 0, 2);
              //          // = BitConverter.ToInt16(_reciveMessageLenght);
              //

                    }
                    Thread.Sleep(500);
                }
            }


        }


    }
}

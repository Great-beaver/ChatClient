using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChatClient
{
    class ByteOperation
    {
        static public short ToShort(short byte1, short byte2)
        {
            return (short)((byte2 << 8) + byte1);
        }

        static public void FromShort(short number, out byte byte1, out byte byte2)
        {
            byte2 = (byte)(number >> 8);
            byte1 = (byte)(number & 255);
        }


        //or
      //  short number = 42;
      //  byte[] numberBytes = BitConverter.GetBytes(number);
      //  short converted = BitConverter.ToInt16(numberBytes);


    }
}

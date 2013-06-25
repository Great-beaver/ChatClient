using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using ProtoBuf;

namespace Chat.Helpers
{
    static public class StructConvertor
    {
       public static byte[] GetBytes<T>(T str)
        {
           using (MemoryStream ms = new MemoryStream())
           {
             Serializer.Serialize(ms, str);

           return ms.ToArray();  
           }
        }

       public static T FromBytes<T>(byte[] bytes) where T : struct
        {
            using (Stream stream = new MemoryStream(bytes))
           {
               T stuff = Serializer.Deserialize<T>(stream);

               return stuff; 
           }

        }
    }
}

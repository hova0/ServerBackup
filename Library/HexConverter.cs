using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EWR.ServerBackup.Library
{
   
    public class HexConverter
    {
        public static string ByteArraytoHex(byte[] buffer)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder(buffer.Length * 2);
            for(int i = 0; i < buffer.Length; i++)
            {
                sb.Append(buffer[i].ToString("X2"));
            }

            return sb.ToString();
        }
        public static byte[] HextoByteArray(string hexvalues)
        {
            if (hexvalues.Contains(" "))
                hexvalues = hexvalues.Replace(" ", "");
            if (hexvalues.Length % 2 == 1)
                throw new Exception("Hex String is not divisible by 2!");
            byte[] buffer = new byte[hexvalues.Length / 2];
            for (int i = 0; i < hexvalues.Length; i+= 2)
            {
                buffer[i/2] = Convert.ToByte(hexvalues.Substring(i, 2), 16);
            }
            return buffer;
        }
       
    }


}

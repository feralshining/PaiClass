using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaiClass
{
    public class PaiCommon
    {
        public static string BytesToHexString(byte[] data)
        {
            return BitConverter.ToString(data).Replace("-", "");
        }

        public static byte[] HexStringToBytes(string hexstring)
        {
            byte[] xbytes = new byte[hexstring.Length / 2];
            for (int i = 0; i < xbytes.Length; i++)
            {
                xbytes[i] = Convert.ToByte(hexstring.Substring(i * 2, 2), 16);
            }

            return xbytes;
        }
    }
}

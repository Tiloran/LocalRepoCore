﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LevelStore.Infrastructure
{
    public static class EncodeToUTF8
    {
        public static string Encode(string stringToEncode)
        {
            if (stringToEncode == null)
            {
                stringToEncode = String.Empty;
            }
            var utf = Encoding.UTF8;
            Byte[] stringBytes = utf.GetBytes(stringToEncode);
            StringBuilder sbBytes = new StringBuilder(stringBytes.Length * 2);
            foreach (byte b in stringBytes)
            {
                sbBytes.AppendFormat("{0:X2}", b);
            }
            stringToEncode = sbBytes.ToString();
            return stringToEncode;
        }
    }
}

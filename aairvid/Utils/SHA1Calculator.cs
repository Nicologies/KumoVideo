using libairvidproto.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace aairvid.Utils
{
    public class SHA1Calculator : ISHA1Calculator
    {
        public byte[] Calculate(byte[] input)
        {
            var cal = System.Security.Cryptography.SHA1.Create();
            return cal.ComputeHash(input);
        }
    }
}

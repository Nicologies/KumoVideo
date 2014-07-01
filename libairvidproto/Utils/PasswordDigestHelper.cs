using libairvidproto.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libairvidproto.Utils
{
    public class PasswordDigestHelper
    {
        public static string GetPasswordHexString(string passwd)
        {
            var bytes = Encoding.UTF8.GetBytes(passwd);

            var sha1 = SHA1Calculator.Instance.Calculate(bytes);

            var hex = BytesToHexString.HexStringFromBytes(sha1);
            return hex;
        }
    }
}

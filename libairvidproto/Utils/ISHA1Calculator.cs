using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libairvidproto.Utils
{
    public interface ISHA1Calculator
    {
        byte[] Calculate(Byte[] input);
    }

    internal static class SHA1Calculator
    {
        public static ISHA1Calculator Instance;
    }
}

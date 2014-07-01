using libairvidproto.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libairvidproto
{
    public static class LibIniter
    {
        public static void Init(IByteOrderConv byteOrderConv, ISHA1Calculator sha1Calc)
        {
            ByteOrderConv.Instance = byteOrderConv;
            SHA1Calculator.Instance = sha1Calc;
        }
    }
}

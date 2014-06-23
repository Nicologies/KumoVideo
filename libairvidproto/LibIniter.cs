using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace libairvidproto
{
    public static class LibIniter
    {
        public static void Init(IByteOrderConv byteOrderConv)
        {
            ByteOrderConv.Instance = byteOrderConv;
        }
    }
}

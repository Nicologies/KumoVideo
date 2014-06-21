using libairvidproto;
using System;
using System.Net;

namespace aairvid
{
    public class ByteOrderConvAdp : IByteOrderConv
    {
        public int HostToNetworkOrder(int host)
        {
            return IPAddress.HostToNetworkOrder(host);
        }

        public long HostToNetworkOrder(long host)
        {
            return IPAddress.HostToNetworkOrder(host);
        }

        public short HostToNetworkOrder(short host)
        {
            return IPAddress.HostToNetworkOrder(host);
        }

        public int NetworkToHostOrder(int network)
        {
            return IPAddress.NetworkToHostOrder(network);
        }

        public long NetworkToHostOrder(long network)
        {
            return IPAddress.NetworkToHostOrder(network);
        }

        public short NetworkToHostOrder(short network)
        {
            return IPAddress.NetworkToHostOrder(network);
        }
    }
}
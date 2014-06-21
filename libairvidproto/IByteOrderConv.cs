
namespace libairvidproto
{
    public interface IByteOrderConv
    {
        int HostToNetworkOrder(int host);
        long HostToNetworkOrder(long host);
        short HostToNetworkOrder(short host);
        int NetworkToHostOrder(int network);
        long NetworkToHostOrder(long network);
        short NetworkToHostOrder(short network);
    }

    public static class ByteOrderConv
    {
        public static IByteOrderConv Instance;
    }
}

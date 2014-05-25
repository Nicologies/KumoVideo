
namespace Network.ZeroConf
{
    public interface IExpirable
    {
        bool IsOutDated { get; }
        uint Ttl { get; }
        void Renew(uint ttl);
    }
}

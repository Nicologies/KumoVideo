
namespace aairvid.Utils
{
    public interface ICodecProfile
    {
        int DeviceHeight
        {
            get;
        }
        int DeviceWidth
        {
            get;
        }

        int Bitrate
        {
            get;
        }
    }
}


namespace aairvid.Utils
{
    public interface ICodecProfile
    {
        int Height
        {
            get;
        }
        int Width
        {
            get;
        }

        int Bitrate
        {
            get;
        }
    }

    public static class EmDefaultBitrate
    {
        public static readonly int WifiRate = 1536;
        public static readonly int MobileDataRate = 384;
    }
}

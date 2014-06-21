using Android.OS;

namespace aairvid.Model
{
    public class StreamBase
    {
        public int index = 0;
        public int StreamType = 0;
        public string Codec = "";
        public string Language = "";

        public StreamBase()
        {
        }

        public StreamBase(Parcel source)
        {
            index = source.ReadInt();
            StreamType = source.ReadInt();
            Codec = source.ReadString();
            Language = source.ReadString();
        }
    }
}

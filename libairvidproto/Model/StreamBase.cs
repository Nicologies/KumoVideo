
namespace libairvidproto.model
{
    public class StreamBase
    {
        public int index
        {
            get;
            set;
        }
        public int StreamType
        {
            get;
            set;
        }
        public string Codec
        {
            get;
            set;
        }
        public string Language
        {
            get;
            set;
        }

        public StreamBase()
        {
            index = 0;
            StreamType = 0;
            Codec = "";
            Language = "";
        }
    }
}

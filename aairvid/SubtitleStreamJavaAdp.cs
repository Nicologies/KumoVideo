
using aairvid.Model;
using libairvidproto.model;

namespace aairvid
{
    public class SubtitleStreamJavaAdp : Java.Lang.Object
    {
        public SubtitleStream Subtitle
        {
            get;
            private set;
        }

        public SubtitleStreamJavaAdp(SubtitleStream stream)
        {
            Subtitle = stream;
        }
    }
}
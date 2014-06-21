
using aairvid.Model;

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
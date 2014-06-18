using aairvid.Model;

namespace aairvid
{
    public class Video : AirVidResource
    {
        public static readonly int ContentType = (int)EmContentType.Video;

        public Video(AirVidServer server, string name, string id)
            : base(server, name, id)
        {
        }

        public Video(Android.OS.Parcel source) : base(source)
        {
        }

        public MediaInfo GetMediaInfo()
        {
            return Server.GetMediaInfo(Id);
        }

        internal string GetPlaybackUrl(MediaInfo mediaInfo, SubtitleStream sub)
        {
            bool noSub = true;

            if(sub != null)
            {
                if(sub.Language == null)
                {
                    noSub = false;
                }
                else if(string.IsNullOrWhiteSpace(sub.Language.Value))
                {
                    noSub = false;
                }
                else if(sub.Language.Value.ToUpperInvariant() != "DISABLED")
                {
                    noSub = false;
                }
            }
            if(!noSub)
            {
                return Server.GetPlayWithConvUrl(this, mediaInfo, sub);
            }
            return Server.GetPlaybackUrl(this);
        }
        
        internal string GetPlayWithConvUrl(MediaInfo mediaInfo, SubtitleStream sub)
        {
            return Server.GetPlayWithConvUrl(this, mediaInfo, sub);
        }

        public override int DescribeContents()
        {
            return ContentType;
        }
    }
}

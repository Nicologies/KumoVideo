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

        internal string GetPlaybackUrl(SubtitleStream sub)
        {
            return Server.GetPlaybackUrl(this, sub);
        }
        
        internal string GetPlayWithConvUrl(SubtitleStream sub)
        {
            return Server.GetPlayWithConvUrl(this, sub);
        }

        public override int DescribeContents()
        {
            return ContentType;
        }
    }
}

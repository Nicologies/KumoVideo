using aairvid.Model;
using aairvid.Utils;
using libairvidproto;

namespace aairvid
{
    public class Video : AirVidResource
    {
        public static readonly int ContentType = (int)EmContentType.Video;

        public Video(AirVidServer server, string name, string id)
            : base(server, name, id)
        {
        }

        public MediaInfo GetMediaInfo(IWebClient webClient)
        {
            return Server.GetMediaInfo(webClient, Id);
        }

        public string GetPlaybackUrl(IWebClient webClient, MediaInfo mediaInfo, SubtitleStream sub, ICodecProfile profile)
        {
            bool noSub = true;

            if (sub != null)
            {
                if (sub.Language == null)
                {
                    noSub = false;
                }
                else if (string.IsNullOrWhiteSpace(sub.Language.Value))
                {
                    noSub = false;
                }
                else if (sub.Language.Value.ToUpperInvariant() != "DISABLED")
                {
                    noSub = false;
                }
            }
            if (!noSub)
            {
                return Server.GetPlayWithConvUrl(webClient, this, mediaInfo, sub, profile);
            }
            return Server.GetPlaybackUrl(webClient, this);
        }

        public string GetPlayWithConvUrl(IWebClient webClient, MediaInfo mediaInfo, SubtitleStream sub, ICodecProfile profile)
        {
            return Server.GetPlayWithConvUrl(webClient, this, mediaInfo, sub, profile);
        }
    }
}

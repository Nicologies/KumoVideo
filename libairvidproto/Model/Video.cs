using aairvid.Utils;

namespace libairvidproto.model
{
    public class Video : AirVidResource
    {
        public static readonly int ContentType = (int)EmContentType.Video;

        public Video(AirVidServer server, string name, string id, AirVidResource parent)
            : base(server, name, id, parent)
        {
        }

        public MediaInfo GetMediaInfo(IWebClient webClient)
        {
            return Server.GetMediaInfo(webClient, this);
        }

        public string GetPlaybackUrl(IWebClient webClient,
            MediaInfo mediaInfo,
            SubtitleStream sub,
            AudioStream audio, 
            ICodecProfile profile)
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
                return Server.GetPlayWithConvUrl(webClient, this, mediaInfo, sub, audio, profile);
            }

            if(audio != null && audio.index != 1)
            {
                return Server.GetPlayWithConvUrl(webClient, this, mediaInfo, sub, audio, profile);
            }
            return Server.GetPlaybackUrl(webClient, this);
        }

        public string GetPlayWithConvUrl(IWebClient webClient, MediaInfo mediaInfo, 
            SubtitleStream sub, AudioStream audio, ICodecProfile profile)
        {
            return Server.GetPlayWithConvUrl(webClient, this, mediaInfo, sub, audio, profile);
        }
    }
}

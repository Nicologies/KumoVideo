

using aairvid.Model;
using libairvidproto.model;
namespace aairvid
{
    interface IPlayVideoListener
    {
        void OnPlayVideo(Video vid, MediaInfo mediaInfo, SubtitleStream sub);

        void OnPlayVideoWithConv(Video vid, MediaInfo mediaInfo, SubtitleStream sub);

        void OnVideoFinished(int playedMinutes);

        void ReloadInterstitialAd();
    }
}
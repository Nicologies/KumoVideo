

using aairvid.Model;
using libairvidproto.model;
namespace aairvid
{
    interface IPlayVideoListener
    {
        void OnPlayVideo(Video vid, MediaInfo mediaInfo, SubtitleStream sub, AudioStream audio);

        void OnPlayVideoWithConv(Video vid, MediaInfo mediaInfo, SubtitleStream sub, AudioStream audio);

        void OnVideoFinished(int playedMinutes);

        void ReloadInterstitialAd();
    }
}
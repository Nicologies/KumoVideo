

using aairvid.Model;
namespace aairvid
{
    interface IPlayVideoListener
    {
        void OnPlayVideo(Video vid, SubtitleStream sub);

        void OnPlayVideoWithConv(Video vid, SubtitleStream sub);
    }
}
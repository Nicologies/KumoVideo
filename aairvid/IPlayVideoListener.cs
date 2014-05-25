

namespace aairvid
{
    interface IPlayVideoListener
    {
        void OnPlayVideo(Video vid);

        void OnPlayVideoWithConv(Video vid);
    }
}
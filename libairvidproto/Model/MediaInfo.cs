using System.Collections.Generic;

namespace libairvidproto.model
{
    public class MediaInfo : AirVidResource
    {
        public static readonly int ContentType = (int)EmContentType.MediaInfo;
        public MediaInfo(AirVidServer server, AirVidResource parent)
            : base(server, "", "", parent)
        {
            FileSize = 0L;
            DurationSeconds = 0f;
            Bitrate = 0;
            Thumbnail = null;
            VideoStreams = new List<VideoStream>();
            AudioStreams = new List<AudioStream>();
            Subtitles = new List<SubtitleStream>();
        }

        public long FileSize { get; set; }
        public double DurationSeconds { get; set; }
        public int Bitrate { get; set; }
        public byte[] Thumbnail { get; set; }
        public List<VideoStream> VideoStreams { get; set; }
        public List<AudioStream> AudioStreams { get; set; }
        public List<SubtitleStream> Subtitles { get; set; }
    }
}

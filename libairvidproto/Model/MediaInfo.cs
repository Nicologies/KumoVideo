using aairvid.Model;
using System.Collections.Generic;

namespace aairvid
{
    public class MediaInfo : AirVidResource
    {
        public static readonly int ContentType = (int)EmContentType.MediaInfo;
        public MediaInfo(AirVidServer server) : base(server, "", "")
        {
        }

        public long FileSize = 0L;
        public double DurationSeconds = 0f;
        public int Bitrate = 0;
        public byte[] Thumbnail = null;
        public List<VideoStream> VideoStreams = new List<VideoStream>();
        public List<AudioStream> AudioStreams = new List<AudioStream>();
        public List<SubtitleStream> Subtitles = new List<SubtitleStream>();
    }
}

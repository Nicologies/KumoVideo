using aairvid.Model;
using Android.OS;
using System.Collections.Generic;
using System.Linq;

namespace aairvid
{
    public class MediaInfo : AirVidResource
    {
        public static readonly int ContentType = (int)EmContentType.MediaInfo;
        public MediaInfo(AirVidServer server) : base(server, "", "")
        {
        }

        public MediaInfo(Parcel source) : base(source)
        {
            FileSize = source.ReadLong();
            Duration = source.ReadDouble();
            Bitrate = source.ReadInt();
            var thumbnailLen = source.ReadInt();
            if (thumbnailLen > 0)
            {
                Thumbnail = new byte[thumbnailLen];
                source.ReadByteArray(Thumbnail);
            }

            VideoStreams = source.ReadParcelableArray(new VideoStream().Class.ClassLoader).Cast<VideoStream>().ToList();
            AudioStreams = source.ReadParcelableArray(new AudioStream().Class.ClassLoader).Cast<AudioStream>().ToList();
        }

        public long FileSize = 0L;
        public double Duration = 0f;
        public int Bitrate = 0;
        public byte[] Thumbnail = null;
        public List<VideoStream> VideoStreams = new List<VideoStream>();
        public List<AudioStream> AudioStreams = new List<AudioStream>();

        public override int DescribeContents()
        {
            return ContentType;
        }

        public override void WriteToParcel(Android.OS.Parcel dest, Android.OS.ParcelableWriteFlags flags)
        {
            base.WriteToParcel(dest, flags);
            dest.WriteLong(FileSize);
            dest.WriteDouble(Duration);
            dest.WriteInt(Bitrate);
            dest.WriteInt(Thumbnail.Length);
            if (Thumbnail.Length > 0)
            {
                dest.WriteByteArray(Thumbnail);
            }
            dest.WriteParcelableArray(VideoStreams.ToArray<Java.Lang.Object>(), flags);
            dest.WriteParcelableArray(AudioStreams.ToArray<Java.Lang.Object>(), flags);
        }
    }
}

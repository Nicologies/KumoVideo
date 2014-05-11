using aairvid.Model;
using Android.OS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace aairvid
{
    public class AVMediaInfo : AVResource
    {
        public static readonly int ContentType = (int)EmContentType.MediaInfo;
        public AVMediaInfo(AVServer server) : base(server, "", "")
        {
        }

        public AVMediaInfo(Parcel source) : base(source)
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

            VideoStreams = source.ReadParcelableArray(new AVVideoStream().Class.ClassLoader).Cast<AVVideoStream>().ToList();
            AudioStreams = source.ReadParcelableArray(new AVAudioStream().Class.ClassLoader).Cast<AVAudioStream>().ToList();
        }

        public long FileSize = 0L;
        public double Duration = 0f;
        public int Bitrate = 0;
        public byte[] Thumbnail = null;
        public List<AVVideoStream> VideoStreams = new List<AVVideoStream>();
        public List<AVAudioStream> AudioStreams = new List<AVAudioStream>();

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

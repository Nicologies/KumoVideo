using Android.OS;
using Java.Interop;

namespace aairvid.Model
{
    public class VideoStream : StreamBase
    {
        public int Width = 0;
        public int Height = 0;

        public VideoStream()
        {
        }

        public VideoStream(Parcel source) : base(source)
        {
            Width = source.ReadInt();
            Height = source.ReadInt();
        }

        public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
        {
            base.WriteToParcel(dest, flags);
            dest.WriteInt(Width);
            dest.WriteInt(Height);
        }

        [ExportField("CREATOR")]
        public static AVVideoStreamCreator InitializeCreator()
        {
            return new AVVideoStreamCreator();
        }
    }

    public class AVVideoStreamCreator : Java.Lang.Object, IParcelableCreator
    {
        public Java.Lang.Object CreateFromParcel(Parcel source)
        {
            return new VideoStream(source);
        }

        public Java.Lang.Object[] NewArray(int size)
        {
            return new VideoStream[size];
        }
    }
}

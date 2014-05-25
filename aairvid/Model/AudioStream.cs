using Android.OS;
using Java.Interop;

namespace aairvid.Model
{
    public class AudioStream : StreamBase
    {
        public AudioStream()
        {
        }
        public AudioStream(Parcel source)
            : base(source)
        {
        }

        [ExportField("CREATOR")]
        public static AVAudioStreamCreator InitializeCreator()
        {
            return new AVAudioStreamCreator();
        }
    }

    public class AVAudioStreamCreator : Java.Lang.Object, IParcelableCreator
    {
        public Java.Lang.Object CreateFromParcel(Parcel source)
        {
            return new AudioStream(source);
        }

        public Java.Lang.Object[] NewArray(int size)
        {
            return new AudioStream[size];
        }
    }
}

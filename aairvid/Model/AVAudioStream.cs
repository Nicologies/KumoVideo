using Android.OS;
using Java.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace aairvid.Model
{
    public class AVAudioStream : AVStreamBase
    {
        public AVAudioStream()
        {
        }
        public AVAudioStream(Parcel source)
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
            return new AVAudioStream(source);
        }

        public Java.Lang.Object[] NewArray(int size)
        {
            return new AVAudioStream[size];
        }
    }
}

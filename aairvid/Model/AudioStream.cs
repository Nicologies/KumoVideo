﻿using Android.OS;
using Java.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace aairvid.Model
{
    public class AudioStream : StreamBase
    {
        public string Language = "";
        public AudioStream()
        {
        }
        public AudioStream(Parcel source)
            : base(source)
        {
            Language = source.ReadString();
        }

        public override void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
        {
            base.WriteToParcel(dest, flags);
            dest.WriteString(Language);
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

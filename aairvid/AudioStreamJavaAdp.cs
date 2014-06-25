using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using libairvidproto.model;

namespace aairvid
{
    public class AudioStreamJavaAdp : Java.Lang.Object
    {
        public AudioStream Stream
        {
            get;
            private set;
        }

        public AudioStreamJavaAdp(AudioStream stream)
        {
            Stream = stream;
        }
    }
}
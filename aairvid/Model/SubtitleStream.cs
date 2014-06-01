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
using aairvid.Protocol;
using Java.Interop;

namespace aairvid.Model
{
    public class SubtitleStream : Java.Lang.Object
    {
        public SubtitleStream()
        {
            Language = "Unknow";
        }
        public string Language
        {
            get;
            set;
        }

        public RootObj SubtitleInfoFromServer
        {
            get;
            set;
        }
    }
}
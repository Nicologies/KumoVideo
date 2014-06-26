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
    public interface IMediaDetailDisplayer
    {
        void DisplayDetail(Video vid, MediaInfo mediaInfo);
    }
}
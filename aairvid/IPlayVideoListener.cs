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

namespace aairvid
{
    interface IPlayVideoListener
    {
        void OnPlayVideo(Video vid);

        void OnPlayVideoWithConv(Video vid);
    }
}
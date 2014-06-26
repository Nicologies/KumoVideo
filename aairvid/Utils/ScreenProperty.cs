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
using Android.Content.Res;
using Android.Util;
using aairvid.Utils;

namespace aairvid.UIUtils
{
    public static class ScreenProperty
    {
        public static bool IsLargeScreen(DisplayMetrics me)
        {
            var widthInches = (float)me.WidthPixels / me.Xdpi;
            return widthInches >= 5.3;//ipad mini width.
        }
    }
}
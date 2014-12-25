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
            var widthInches = (float)me.WidthPixels / (float)me.DensityDpi;
            return widthInches >= 5.3;//ipad mini width.
        }
        public static KeyValuePair<int, int> GetScreenResolution(Context ctx)
        {
            var x = IO.Vov.Vitamio.Utils.ScreenResolution.GetResolution(ctx);
            var w = x.First as Java.Lang.Integer;
            var h = x.Second as Java.Lang.Integer;
            return new KeyValuePair<int, int>(w.IntValue(), h.IntValue());
        }
    }
}
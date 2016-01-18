using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Android.Util;

namespace aairvid.UIUtils
{
    public static class ScreenProperty
    {
        public static bool IsLargeScreen(DisplayMetrics me)
        {
            var widthInches = me.WidthPixels/(float) me.DensityDpi;
            return widthInches >= 5.3; //ipad mini width.
        }

        public static KeyValuePair<int, int> GetScreenResolution(Context ctx)
        {
            var wm = (IWindowManager)ctx.GetSystemService(Context.WindowService);
            var display = wm.DefaultDisplay;
            var metrics = new DisplayMetrics();
            display.GetRealMetrics(metrics);
            return new KeyValuePair<int, int>(metrics.WidthPixels, metrics.HeightPixels);
        }
    }
}
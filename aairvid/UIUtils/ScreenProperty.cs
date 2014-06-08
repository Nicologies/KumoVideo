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

namespace aairvid.UIUtils
{
    public static class ScreenProperty
    {
        public static bool IsLargeScreen(int width)
        {
            return width >= 1024;
        }
    }
}
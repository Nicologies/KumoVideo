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
using System.Text.RegularExpressions;
using Android.Util;
using Android.Preferences;

namespace aairvid.Utils
{
    public class AndroidCodecProfile : ICodecProfile
    {
        private static AndroidCodecProfile profile;
        public static AndroidCodecProfile InitProfile(Activity activity)
        {
            if (profile == null)
            {
                profile = new AndroidCodecProfile();
                var pref = PreferenceManager.GetDefaultSharedPreferences(activity);
				profile.Bitrate = int.Parse(pref.GetString("KeyBitRateWifi", "1536"));

                try
                {
                    // hack to get the real device metrics
                    var dispStr = activity.WindowManager.DefaultDisplay.ToString();
                    //"Display id 0: DisplayInfo{\"Built-in Screen\", app 800 x 1216, real 800 x 1280, largest app 1280 x 1183, smallest app 800 x 703, 60.
                    Regex regex = new Regex(@".*real (\d+) x (\d+),.*", RegexOptions.IgnoreCase);
                    var matches = regex.Match(dispStr);
                    if (matches.Success)
                    {
                        var w = int.Parse(matches.Groups[1].Value);
                        var h = int.Parse(matches.Groups[2].Value);
                        // self, height, width
                        profile.DeviceHeight = Math.Min(w, h); ;
                        profile.DeviceWidth = Math.Max(w, h);
                        return profile;
                    }
                }
                catch
                {// fall through to use the DisplayMetrics if any exception.
                }

                DisplayMetrics localDisplayMetrics = activity.Resources.DisplayMetrics;
                int height = Math.Min(localDisplayMetrics.WidthPixels, localDisplayMetrics.HeightPixels);
                profile.DeviceHeight = height;

                int width = Math.Max(localDisplayMetrics.HeightPixels, localDisplayMetrics.WidthPixels);
                profile.DeviceWidth = width;
            }
            return profile;
        }

        public static AndroidCodecProfile GetProfile()
        {
            return profile;
        }

        public int DeviceHeight
        {
            get;
            private set;
        }
        public int DeviceWidth
        {
            get;
            private set;
        }

        public int Bitrate
        {
            get;
            private set;
        }
    }
}
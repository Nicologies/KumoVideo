using Android.App;
using Android.Util;
using System;
using System.Text.RegularExpressions;

namespace aairvid.Utils
{
    public class CodecProfile
    {
        private static CodecProfile profile;
        public static CodecProfile InitProfile(Activity activity)
        {
            if(profile ==null)
            {
                profile = new CodecProfile();
                profile.Bitrate = 1024;

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

        public static CodecProfile GetProfile()
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

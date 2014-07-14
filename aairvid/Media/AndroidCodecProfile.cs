using aairvid.Settings;
using Android.App;
using Android.Content;
using Android.Net;
using Android.Preferences;
using Android.Util;
using System;
using System.Text.RegularExpressions;

namespace aairvid.Utils
{
    public class AndroidCodecProfile : ICodecProfile
    {
        private static AndroidCodecProfile profile;
        private Activity _activity;
        public static AndroidCodecProfile InitProfile(Activity activity)
        {
            if (profile != null)
            {
                profile._activity = activity;
            }
            if (profile == null)
            {
                profile = new AndroidCodecProfile();
                profile._activity = activity;
                var pref = PreferenceManager.GetDefaultSharedPreferences(activity);

                profile.BitrateWifi = pref.GetBitRateWifi(activity.Resources);

                profile.Bitrate3G = pref.GetBitRate3G(activity.Resources);

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
                        profile.DeviceHeight = Math.Min(w, h);
                        profile.DeviceWidth = Math.Max(w, h);

                        DoGenCodecProfile(activity, pref);

                        return profile;
                    }
                }
                catch
                {// fall through to use the DisplayMetrics if any exception.
                }

                DisplayMetrics localDisplayMetrics = activity.Resources.DisplayMetrics;
                profile.DeviceHeight = Math.Min(localDisplayMetrics.WidthPixels, localDisplayMetrics.HeightPixels);
                profile.DeviceWidth = Math.Max(localDisplayMetrics.HeightPixels, localDisplayMetrics.WidthPixels);

                DoGenCodecProfile(activity, pref);
                
            }
            return profile;
        }

        private static void DoGenCodecProfile(Activity activity, ISharedPreferences pref)
        {
            profile.HeightWifi = profile.DeviceHeight;
            profile.WidthWifi = profile.DeviceWidth;
            profile.HeightWifi = pref.GetCodecHeightWifi(activity.Resources, profile.HeightWifi);
            profile.WidthWifi = pref.GetCodecWidthWifi(activity.Resources, profile.WidthWifi);
            int defaultWidth3G = 480;
            profile.Width3G = pref.GetCodecWidth3G(activity.Resources, defaultWidth3G);
            int desiredHeight = (int)((float)defaultWidth3G * ((float)profile.DeviceHeight / (float)profile.DeviceWidth));
            profile.Height3G = pref.GetCodecHeight3G(activity.Resources, desiredHeight);
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

        public static AndroidCodecProfile GetProfile()
        {
            return profile;
        }

        public int Height
        {
            get
            {
                return IsWifiEnabled() ? HeightWifi : Height3G;
            }
        }
        public int Width
        {
            get
            {
                return IsWifiEnabled() ? WidthWifi : Width3G;
            }
        }

        private int HeightWifi
        {
            get;
            set;
        }
        private int WidthWifi
        {
            get;
            set;
        }

        private int Height3G
        {
            get;
            set;
        }
        private int Width3G
        {
            get;
            set;
        }

        protected int BitrateWifi
        {
            get;
            set;
        }

        protected int Bitrate3G
        {
            get;
            set;
        }

        public int Bitrate
        {
            get
            {
                return IsWifiEnabled() ? BitrateWifi : Bitrate3G;
            }
        }

        private bool IsWifiEnabled()
        {
            var connectivityManager = (ConnectivityManager)_activity.GetSystemService(
                Context.ConnectivityService);

            var wifiState = connectivityManager.GetNetworkInfo(ConnectivityType.Wifi)
                .GetState();
            var wifiEnabled = wifiState == NetworkInfo.State.Connected;
            return wifiEnabled;
        }
    }
}
using aairvid.Settings;
using aairvid.UIUtils;
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
                    var res = ScreenProperty.GetScreenResolution(activity);
                    var w = res.Key;
                    var h = res.Value;
                    // self, height, width
                    profile.DeviceHeight = Math.Min(w, h);
                    profile.DeviceWidth = Math.Max(w, h);

                    DoGenCodecProfile(activity, pref);

                    return profile;
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
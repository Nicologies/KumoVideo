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

                try
                {
                    var res = ScreenProperty.GetScreenResolution(activity);
                    var w = res.Key;
                    var h = res.Value;
                    // self, height, width
                    profile.DeviceHeight = Math.Min(w, h);
                    profile.DeviceWidth = Math.Max(w, h);

                    DoGenCodecProfile(activity);

                    return profile;
                }
                catch
                {// fall through to use the DisplayMetrics if any exception.
                }

                DisplayMetrics localDisplayMetrics = activity.Resources.DisplayMetrics;
                profile.DeviceHeight = Math.Min(localDisplayMetrics.WidthPixels, localDisplayMetrics.HeightPixels);
                profile.DeviceWidth = Math.Max(localDisplayMetrics.HeightPixels, localDisplayMetrics.WidthPixels);

                DoGenCodecProfile(activity);
                
            }
            return profile;
        }

        private static void DoGenCodecProfile(Activity activity)
        {
            var pref = PreferenceManager.GetDefaultSharedPreferences(activity);

            pref.DefaultsBitRateWifi(activity.Resources);

            pref.DefaultsBitRate3G(activity.Resources);

            pref.DefaultsCodecHeightWifi(activity.Resources, profile.DeviceHeight);
            pref.DefaultsCodecWidthWifi(activity.Resources, profile.DeviceWidth);
            int defaultWidth3G = 480;
            pref.DefaultsCodecWidth3G(activity.Resources, defaultWidth3G);
            int desiredHeight = (int)((float)defaultWidth3G * ((float)profile.DeviceHeight / (float)profile.DeviceWidth));
            pref.DefaultsCodecHeight3G(activity.Resources, desiredHeight);
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
                var pref = PreferenceManager.GetDefaultSharedPreferences(_activity);
                if (IsWifiEnabled())
                {
                    return pref.GetCodecHeightWifi(_activity.Resources, DeviceHeight);
                }
                else
                {
                    return pref.GetCodecHeight3G(_activity.Resources, 320);
                }
            }
        }
        public int Width
        {
            get
            {
                var pref = PreferenceManager.GetDefaultSharedPreferences(_activity);
                if (IsWifiEnabled())
                {
                    return pref.GetCodecWidthWifi(_activity.Resources, DeviceWidth);
                }
                else
                {
                    return pref.GetCodecWidth3G(_activity.Resources, 480);
                }
            }
        }

        public int Bitrate
        {
            get
            {
                var pref = PreferenceManager.GetDefaultSharedPreferences(_activity);
                if (IsWifiEnabled())
                {
                    return pref.GetBitRateWifi(_activity.Resources);
                }
                else
                {
                    return pref.GetBitRate3G(_activity.Resources);
                }
            }
        }

        private bool IsWifiEnabled()
        {
            return this._activity.IsWifiEnabled();
        }

        public bool H264Passthrough
        {
            get
            {
                var pref = PreferenceManager.GetDefaultSharedPreferences(_activity);
                if (IsWifiEnabled())
                {
                    return pref.GetH264PassthroughWifi(_activity.Resources);
                }
                else
                {
                    return pref.GetH264Passthrough3G(_activity.Resources);
                }
            }
        }
    }
}
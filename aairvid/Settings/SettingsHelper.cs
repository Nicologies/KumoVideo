using aairvid.Utils;
using Android.Content;
using Android.Content.Res;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace aairvid.Settings
{
    public static class SettingsHelper
    {
        public static int GetBitRateWifi(this ISharedPreferences pref, Resources res)
        {
            var key = res.GetString(Resource.String.KeyBitRateWifi);
            var defaultValue =EmDefaultBitrate.WifiRate;
            return DoGetIntValue(pref, key, defaultValue);
        }

        public static int GetBitRate3G(this ISharedPreferences pref, Resources res)
        {
            var key = res.GetString(Resource.String.KeyBitRate3G);
            int defaultValue = EmDefaultBitrate.MobileDataRate;

            var ret = DoGetIntValue(pref, key, defaultValue);
            return ret;
        }

        public static void DefaultsBitRate3G(this ISharedPreferences pref, Resources res)
        {
            var key = res.GetString(Resource.String.KeyBitRate3G);
            int defaultValue = EmDefaultBitrate.MobileDataRate;
            DoSetIntValueIfNotExisting(pref, key, defaultValue);
        }

        public static void DefaultsBitRateWifi(this ISharedPreferences pref, Resources res)
        {
            var key = res.GetString(Resource.String.KeyBitRateWifi);
            int defaultValue = EmDefaultBitrate.WifiRate;
            DoSetIntValueIfNotExisting(pref, key, defaultValue);
        }

        public static int GetCodecWidthWifi(this ISharedPreferences pref, Resources res, int defaultValue)
        {
            var key = res.GetString(Resource.String.KeyCodecWidthWifi);
            return DoGetIntValue(pref, key, defaultValue);
        }

        public static int GetCodecHeightWifi(this ISharedPreferences pref, Resources res, int defaultValue)
        {
            var key = res.GetString(Resource.String.KeyCodecHeightWifi);

            return DoGetIntValue(pref, key, defaultValue);
        }

        public static void DefaultsCodecHeightWifi(this ISharedPreferences pref, Resources res, int defaultValue)
        {
            var key = res.GetString(Resource.String.KeyCodecHeightWifi);
            DoSetIntValueIfNotExisting(pref, key, defaultValue);
        }

        public static void DefaultsCodecWidthWifi(this ISharedPreferences pref, Resources res, int defaultValue)
        {
            var key = res.GetString(Resource.String.KeyCodecWidthWifi);
            DoSetIntValueIfNotExisting(pref, key, defaultValue);
        }

        public static int GetCodecWidth3G(this ISharedPreferences pref, Resources res, int defaultValue)
        {
            var key = res.GetString(Resource.String.KeyCodecWidth3G);

            return DoGetIntValue(pref, key, defaultValue);
        }

        public static int GetCodecHeight3G(this ISharedPreferences pref, Resources res, int defaultValue)
        {
            var key = res.GetString(Resource.String.KeyCodecHeight3G);

            return DoGetIntValue(pref, key, defaultValue);
        }

        public static void DefaultsCodecHeight3G(this ISharedPreferences pref, Resources res, int defaultValue)
        {
            var key = res.GetString(Resource.String.KeyCodecHeight3G);
            DoSetIntValueIfNotExisting(pref, key, defaultValue);
        }

        public static void DefaultsCodecWidth3G(this ISharedPreferences pref, Resources res, int defaultValue)
        {
            var key = res.GetString(Resource.String.KeyCodecWidth3G);
            DoSetIntValueIfNotExisting(pref, key, defaultValue);
        }

        private static void DoSetIntValueIfNotExisting(ISharedPreferences pref, string key, int value)
        {
            if (!pref.Contains(key))
            {
                var editor = pref.Edit();
                editor.PutString(key, value.ToString());
                editor.Commit();
            }
        }

        private static int DoGetIntValue(ISharedPreferences pref, string key, int defaultValue)
        {
            var ret = int.Parse(pref.GetString(key, defaultValue.ToString()));
            return ret;
        }

        public static bool GetH264PassthroughWifi(this ISharedPreferences pref, Resources res)
        {
            var key = res.GetString(Resource.String.KeyH264PassthroughWifi);
            bool defaultValue = false;

            var ret = pref.GetBoolean(key, defaultValue);
            return ret;
        }

        public static bool GetH264Passthrough3G(this ISharedPreferences pref, Resources res)
        {
            var key = res.GetString(Resource.String.KeyH264Passthrough3G);
            bool defaultValue = false;

            var ret = pref.GetBoolean(key, defaultValue);
            return ret;
        }

        public static void SetH264PassthroughWifi(this ISharedPreferences pref, Resources res, bool value)
        {
            var key = res.GetString(Resource.String.KeyH264PassthroughWifi);
            var editor = pref.Edit();
            editor.PutBoolean(key, value);
            editor.Commit();
        }

        public static void SetH264Passthrough3G(this ISharedPreferences pref, Resources res, bool value)
        {
            var key = res.GetString(Resource.String.KeyH264Passthrough3G);
            var editor = pref.Edit();
            editor.PutBoolean(key, value);
            editor.Commit();
        }

        public static bool ShouldShowH264Warning(this ISharedPreferences pref, Resources res)
        {
            var key = res.GetString(Resource.String.KeyShowH264Warning);
            return pref.GetBoolean(key, true);
        }

        public static void SetShouldShowH264Warning(this ISharedPreferences pref, Resources res, bool value)
        {
            var key = res.GetString(Resource.String.KeyShowH264Warning);
            var editor = pref.Edit();
            editor.PutBoolean(key, value);
            editor.Commit();
        }

        public static bool PreferPersonalizedAds(this ISharedPreferences pref, Resources res)
        {
            var key = res.GetString(Resource.String.KeyPreferPersonalizedAds);
            return pref.GetBoolean(key, true);
        }

        public static void SetAdsPreference(this ISharedPreferences pref, Resources res, bool preferPersonalized)
        {
            var key = res.GetString(Resource.String.KeyPreferPersonalizedAds);
            var editor = pref.Edit();
            editor.PutBoolean(key, preferPersonalized);
            editor.Commit();
        }

        public static bool IsConsentCollected(this ISharedPreferences pref, Resources res)
        {
            var key = res.GetString(Resource.String.KeyConsentCollected);
            return pref.GetBoolean(key, false);
        }
    }
}

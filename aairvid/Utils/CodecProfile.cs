using Android.App;
using Android.Media;
using Android.Util;
using Android.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

                DisplayMetrics localDisplayMetrics = activity.Resources.DisplayMetrics;
                int m = Math.Min(localDisplayMetrics.WidthPixels, localDisplayMetrics.HeightPixels);
                profile.Height = 288;

                int k = Math.Max(localDisplayMetrics.HeightPixels, localDisplayMetrics.WidthPixels);
                profile.Width = 352;

                profile.Bitrate = 1024;
            }
            return profile;
        }

        public static CodecProfile GetProfile()
        {
            return profile;
        }

        public int Height
        {
            get;
            private set;
        }
        public int Width
        {
            get;
            private set;
        }

        public int Bitrate
        {
            get;
            private set;
        }
        private static CamcorderProfile GetDevProfile()
        {
            var profile = CamcorderProfile.Get(CamcorderQuality.High);
            if (profile != null)
            {
                return profile;
            }
            profile = CamcorderProfile.Get(CamcorderQuality.Q1080p);
            if (profile != null)
            {
                return profile;
            }

            profile = CamcorderProfile.Get(CamcorderQuality.Q720p);
            if (profile != null)
            {
                return profile;
            }

            profile = CamcorderProfile.Get(CamcorderQuality.Q480p);
            if (profile != null)
            {
                return profile;
            }

            profile = CamcorderProfile.Get(CamcorderQuality.Cif);
            if (profile != null)
            {
                return profile;
            }

            profile = CamcorderProfile.Get(CamcorderQuality.Qcif);
            if (profile != null)
            {
                return profile;
            }
            profile = CamcorderProfile.Get(CamcorderQuality.Low);
            if (profile != null)
            {
                return profile;
            }

            return null;
        }
    }
}

using Android.Content;
using Android.Gms.Ads;
using Android.Preferences;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace aairvid.Ads
{
    public class AdsLayout : LinearLayout
    {
        public static readonly string ResumeFromAdClicked = "AdsLayout.AdsClicked";
        public static readonly string NoAdsHours = "AdsLayout.NoAdsHours";
        public static readonly string NoAdsFrom = "AdsLayout.NoAdsFrom";
        public static readonly string NoAdsDateFmt = "dd/MM/yyyy HH:mm:ss";
        private static readonly int[] Weights = 
        {
            4,4,4,4,4,4,4,4,4,4,4,4,4,4,4,4,4,4,4,4,4,4,4 ,4,4,
            8,8,8,8,8,8,8,8,8,8,8,8,8,
            15,15,15,15,15,15,15,
            16,16,16,16,16,16,16,
            23,23,23,
            42,
        };

        private AdView _ad;

        private bool _adsMightClicked;
        public AdsLayout(Context context)
            : base(context)
        {

        }

        public AdsLayout(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {

        }

        public AdsLayout(Context context, IAttributeSet attrs, int defStyle)
            : base(context, attrs, defStyle)
        {

        }
        protected AdsLayout(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }
        public override bool OnInterceptTouchEvent(Android.Views.MotionEvent ev)
        {
            if (ev.Action == MotionEventActions.Up)
            {
                _adsMightClicked = true;
            }
            return base.OnInterceptTouchEvent(ev);
        }

        public void ResetAdsClickStatus()
        {
            _adsMightClicked = false;
        }

        private void ResetAdsStatus()
        {
            if (_ad?.AdListener != null)
            {
                var listener = _ad.AdListener as AdListenerImpl;
                _ad.AdListener = null;
                listener?.Dispose();
            }
            _ad = null;
            IsAdsLoaded = false;
        }

        protected override Android.OS.IParcelable OnSaveInstanceState()
        {
            if (_adsMightClicked)
            {
                var pref = PreferenceManager.GetDefaultSharedPreferences(Context);
                var editor = pref.Edit();
                editor.PutBoolean(ResumeFromAdClicked, true);
                editor.Commit();
            }
            return base.OnSaveInstanceState();
        }

        public static void SaveNoAdsPref(ISharedPreferences pref)
        {
            var rand = new Random().Next();
            System.Diagnostics.Trace.TraceInformation("rand, {0}", rand % 100);
            var hit = rand % 100 <= 5;
            if (!hit) return;
            var noAdsIndex = new Random().Next() % Weights.Length;
            var editor = pref.Edit();
            editor.PutInt(AdsLayout.NoAdsHours, Weights[noAdsIndex]);
            editor.PutString(AdsLayout.NoAdsFrom, DateTime.Now.ToString(NoAdsDateFmt));
            editor.PutBoolean(AdsLayout.ResumeFromAdClicked, false);
            editor.Commit();
        }

        protected override void OnDetachedFromWindow()
        {
            ResetAdsStatus();
            base.OnDetachedFromWindow();
        }

        public void LoadAds()
        {
            if (IsAdsLoaded)
            {
                return;
            }
            var pref = PreferenceManager.GetDefaultSharedPreferences(Context);
            var resumeFromAdsClicked = pref.GetBoolean(AdsLayout.ResumeFromAdClicked, false);
            if (resumeFromAdsClicked)
            {
                SaveNoAdsPref(pref);
            }

            if (ShouldShowAds(pref))
            {
                if (_ad != null) return;
                _ad = new AdView(Context)
                {
                    AdSize = AdSize.SmartBanner,
                    AdUnitId = "ca-app-pub-3312616311449672/9767882743",
                    Id = Resource.Id.adView
                };


                var layoutParams = new LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);

                _ad.LayoutParameters = layoutParams;

                var adRequest = new AdRequest.Builder()
                    .AddTestDevice(AdRequest.DeviceIdEmulator)
                    .AddTestDevice("45A39B3DBAE829B7AB8BA9B2C9E55D6F")
                    .AddTestDevice("421746E519013F2F4FF3B62742A642D1")
                    .AddTestDevice("61B125201311D25A92623D5862F94D9A")
                    .Build();

                _ad.AdListener = new AdListenerImpl(this, _ad, adRequest);
                _ad.LoadAd(adRequest);
            }
            else
            {
                if (resumeFromAdsClicked)
                {
                    var noAdsMin = pref.GetInt(NoAdsHours, 0);
                    if (noAdsMin == Weights.Last())
                    {
                        var bigDay = this.Resources.GetString(aairvid.Resource.String.ThanksForClickingAdsBigDay);
                        var txt = noAdsMin.ToString() + ": " + bigDay;
                        Toast.MakeText(Context,
                            txt,
                            ToastLength.Long)
                            .Show();
                    }
                    else
                    {
                        var thanks = this.Resources.GetString(aairvid.Resource.String.ThanksForClickingAds);
                        var txt = noAdsMin.ToString() + ": " + thanks;
                        Toast.MakeText(Context,
                            txt,
                            ToastLength.Long).Show();
                    }
                }
                this.Visibility = ViewStates.Gone;
            }
        }

        private static bool ShouldShowAds(ISharedPreferences pref)
        {
#if DEBUG
            return true;
#endif
#pragma warning disable 162
            var noAdsHours = pref.GetInt(NoAdsHours, 0);
            var noAdsFromStr = pref.GetString(NoAdsFrom, DateTime.Now.ToString(NoAdsDateFmt));
            var noAdsFrom = DateTime.ParseExact(noAdsFromStr, NoAdsDateFmt, CultureInfo.InvariantCulture);

            var now = DateTime.Now;

            return noAdsFrom + TimeSpan.FromHours(noAdsHours * 5) <= now;
#pragma warning restore 162
        }

        public static readonly bool ShowAdsWhenPlaying = false;
        public bool IsAdsLoaded { get; set; }
    }

    public class AdListenerImpl : AdListener, IDisposable
    {
        private const int InitDelay = 5000;
        private static int _delay = InitDelay; 
        private AdsLayout _adContainer;
        private AdView _ad;
        private readonly AdRequest _adRequest;

        public AdListenerImpl(AdsLayout adsLayout, AdView ad, AdRequest adRequest)
        {
            _adContainer = adsLayout;
            _ad = ad;
            _adRequest = adRequest;
        }
        public override void OnAdLoaded()
        {
            _delay = InitDelay;
            _adContainer.RemoveAllViews();
            _adContainer.AddView(_ad);
            _adContainer.IsAdsLoaded = true;
        }

        public override void OnAdFailedToLoad(int p0)
        {
            base.OnAdFailedToLoad(p0);

            ReloadWithDelay();

            _delay *= 2;
        }

        private async void ReloadWithDelay()
        {
            await Task.Delay(_delay);
            _ad.LoadAd(_adRequest);
        }

        void IDisposable.Dispose()
        {
            _ad = null;
            _adContainer = null;
        }
    }
}

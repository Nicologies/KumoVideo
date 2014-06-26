using Android.Gms.Ads;
using Android.Content;
using Android.Preferences;
using Android.Util;
using Android.Widget;
using Android.Views;
using System;
using System.Linq;
using System.Globalization;
using System.Threading.Tasks;

namespace aairvid.UIUtils
{
    public class AdsLayout : LinearLayout
    {
        public static readonly string RESUME_FROM_AD_CLICKED = "AdsLayout.AdsClicked";
        public static readonly string NO_ADS_HOURS = "AdsLayout.NoAdsHours";
        public static readonly string NO_ADS_FROM = "AdsLayout.NoAdsFrom";
        private static readonly string NO_ADS_DATE_FMT = "dd/MM/yyyy HH:mm:ss";
        private static readonly int[] Weights = 
        {
            4,4,4,4,4,4,4,4,4,4,4,4,4,4,4,4,4,4,4,4,4,4,4 ,4,4,
            8,8,8,8,8,8,8,8,8,8,8,8,8,
            15,15,15,15,15,15,15,
            16,16,16,16,16,16,16,
            23,23,23,
            42,
        };

        AdView ad;

        private bool _adsMightClicked = false;
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
            if (ad != null)
            {
                if (ad.AdListener != null)
                {
                    var listener = ad.AdListener as AdListenerImpl;
                    ad.AdListener = null;
                    listener.Dispose();
                }
            }
            ad = null;
            IsAdsLoaded = false;
        }

        protected override Android.OS.IParcelable OnSaveInstanceState()
        {
            if (_adsMightClicked)
            {
                var pref = PreferenceManager.GetDefaultSharedPreferences(Context);
                var editor = pref.Edit();
                editor.PutBoolean(RESUME_FROM_AD_CLICKED, true);
                editor.Commit();
            }
            return base.OnSaveInstanceState();
        }

        public static void SaveNoAdsPref(ISharedPreferences pref)
        {
            var rand = new Random().Next();
            System.Diagnostics.Trace.TraceInformation("rand, {0}", rand % 100);
            bool hit = rand % 100 <= 5;
            if (hit)
            {
                int noAdsIndex = new Random().Next() % Weights.Length;
                var editor = pref.Edit();
                editor.PutInt(AdsLayout.NO_ADS_HOURS, Weights[noAdsIndex]);
                editor.PutString(AdsLayout.NO_ADS_FROM, DateTime.Now.ToString(NO_ADS_DATE_FMT));
                editor.PutBoolean(AdsLayout.RESUME_FROM_AD_CLICKED, false);
                editor.Commit();
            }
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
            var resumeFromAdsClicked = pref.GetBoolean(AdsLayout.RESUME_FROM_AD_CLICKED, false);
            if (resumeFromAdsClicked)
            {
                SaveNoAdsPref(pref);
            }

            if (ShouldShowAds(pref))
            {
                if (ad == null)
                {
                    ad = new AdView(Context);

                    ad.AdSize = AdSize.SmartBanner;
                    ad.AdUnitId = "ca-app-pub-3312616311449672/9767882743";
                    ad.Id = Resource.Id.adView;

                    var layoutParams = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.FillParent);

                    ad.LayoutParameters = layoutParams;

                    AdRequest adRequest = new AdRequest.Builder()
                        .AddTestDevice(AdRequest.DeviceIdEmulator)
                        .AddTestDevice("421746E519013F2F4FF3B62742A642D1")
                        .AddTestDevice("61B125201311D25A92623D5862F94D9A")
                        .Build();

                    ad.AdListener = new AdListenerImpl(this, ad, adRequest);
                    ad.LoadAd(adRequest);
                }
            }
            else
            {
                if (resumeFromAdsClicked)
                {
                    var noAdsMin = pref.GetInt(NO_ADS_HOURS, 0);
                    if (noAdsMin == Weights.Last())
                    {
                        var bigDay = this.Resources.GetString(aairvid.Resource.String.ThanksForClickingAdsBigDay);
                        string txt = noAdsMin.ToString() + ": " + bigDay;
                        Toast.MakeText(Context,
                            txt,
                            ToastLength.Long)
                            .Show();
                    }
                    else
                    {
                        var thanks = this.Resources.GetString(aairvid.Resource.String.ThanksForClickingAds);
                        string txt = noAdsMin.ToString() + ": " + thanks;
                        Toast.MakeText(Context,
                            txt,
                            ToastLength.Long).Show();
                    }
                }
                this.Visibility = ViewStates.Gone;
            }
        }

        private bool ShouldShowAds(ISharedPreferences pref)
        {
#if DEBUG
            return true;
#endif
            var noAdsHours = pref.GetInt(NO_ADS_HOURS, 0);
            var noAdsFromStr = pref.GetString(NO_ADS_FROM, DateTime.Now.ToString(NO_ADS_DATE_FMT));
            var noAdsFrom = DateTime.ParseExact(noAdsFromStr, NO_ADS_DATE_FMT, CultureInfo.InvariantCulture);

            var now = DateTime.Now;

            if (noAdsFrom + TimeSpan.FromHours(noAdsHours * 5) > now)
            {
                return false;
            }
            return true;
        }

        private bool _IsAdsLoaded = false;
        public static readonly bool SHOW_ADS_WHEN_PLAYING = false;
        public bool IsAdsLoaded
        {
            get
            {
                return _IsAdsLoaded;
            }
            set
            {
                _IsAdsLoaded = value;
            }
        }
    }

    public class AdListenerImpl : AdListener, IDisposable
    {
        private AdsLayout _adContainer;
        private AdView _ad;
        private AdRequest _adRequest;

        public AdListenerImpl(AdsLayout adsLayout, AdView ad, AdRequest adRequest)
        {
            this._adContainer = adsLayout;
            this._ad = ad;
            this._adRequest = adRequest;
        }
        public override void OnAdLoaded()
        {
            _adContainer.RemoveAllViews();
            _adContainer.AddView(_ad);
            _adContainer.IsAdsLoaded = true;
        }

        public override void OnAdFailedToLoad(int p0)
        {
            base.OnAdFailedToLoad(p0);

            ReloadWithDelay();
        }

        private async void ReloadWithDelay()
        {
            await Task.Delay(5000);
            _ad.LoadAd(_adRequest);
        }

        void IDisposable.Dispose()
        {
            _ad = null;
            _adContainer = null;
        }
    }
}

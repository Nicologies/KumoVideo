using Android.Gms.Ads;
using Android.Runtime;
using System;

namespace aairvid.Ads
{
    public class InterstitialAdImpl : AdListener, IDisposable
    {
        private const int InitDelay = 5000;
        private static int _delay = InitDelay; 
        InterstitialAd _ad;
        public InterstitialAdImpl(InterstitialAd ad)
        {
            _ad = ad;
        }

        protected InterstitialAdImpl(IntPtr javaReference,
            JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }
        public override void OnAdClosed()
        {
            base.OnAdClosed();
            ReloadAds();
        }
        public override void OnAdLoaded()
        {
            _delay = InitDelay;
            base.OnAdLoaded();
        }
        public override void OnAdFailedToLoad(int p0)
        {
            base.OnAdFailedToLoad(p0);
            ReloadAds();
            _delay *= 2;
        }

        private async void ReloadAds()
        {
            await System.Threading.Tasks.Task.Delay(_delay);
            if (_ad != null)
            {
                _ad.LoadAd(new AdRequest.Builder()
                   .AddTestDevice("45A39B3DBAE829B7AB8BA9B2C9E55D6F")
                   .AddTestDevice("421746E519013F2F4FF3B62742A642D1")
                   .AddTestDevice("61B125201311D25A92623D5862F94D9A")
                   .Build());
            }
        }

        void IDisposable.Dispose()
        {
            _ad = null;
        }
    }
}

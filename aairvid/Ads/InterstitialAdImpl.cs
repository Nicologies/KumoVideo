using Android.Gms.Ads;
using System;

namespace aairvid.Ads
{
    public class InterstitialAdImpl : AdListener, IDisposable
    {
        InterstitialAd _ad;
        public InterstitialAdImpl(InterstitialAd ad)
        {
            _ad = ad;
        }
        public override void OnAdClosed()
        {
            base.OnAdClosed();
            ReloadAds();
        }
        public override void OnAdLoaded()
        {
            base.OnAdLoaded();
        }
        public override void OnAdFailedToLoad(int p0)
        {
            base.OnAdFailedToLoad(p0);
            ReloadAds();
        }

        private async void ReloadAds()
        {
            await System.Threading.Tasks.Task.Delay(5000);
            if (_ad != null)
            {
                _ad.LoadAd(new AdRequest.Builder()
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

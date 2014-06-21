using aairvid.Adapter;
using aairvid.Fragments;
using aairvid.Model;
using aairvid.UIUtils;
using aairvid.Utils;
using Android.App;
using Android.Content;
using Android.Gms.Ads;
using Android.Gms.Analytics;
using Android.Net;
using Android.OS;
using Android.Preferences;
using Android.Provider;
using Android.Widget;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using System.Text;
using Android.Runtime;
using Android.Util;

namespace aairvid
{
    [Activity(Label = "aairvid"
        , MainLauncher = false
        ,Icon = "@drawable/icon"
        ,NoHistory = false
        ,ScreenOrientation= Android.Content.PM.ScreenOrientation.SensorLandscape
        ,ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize
        )]
    public class MainActivity : Activity, IResourceSelectedListener, IPlayVideoListener, IServerSelectedListener, IVideoNotPlayableListener
    {
        ServersFragment _serverFragment;

        private bool killed = false;

        private int exitCounter = 0;

        private AdsLayout _adsLayout;

        private InterstitialAd _fullScreenAds;
        public enum EmTraker
        {
            GlobalTracker
        };
        Dictionary<EmTraker, Tracker> _trackers = new Dictionary<EmTraker, Tracker>();

        protected override void OnDestroy()
        {
            killed = true;
            ResetFullScreenAds();

            base.OnDestroy();
        }

        private void ResetFullScreenAds()
        {
            if (_fullScreenAds != null)
            {
                var listener = _fullScreenAds.AdListener as InterstitialAdImpl;
                _fullScreenAds.AdListener = null;
                _fullScreenAds = null;

                if (listener != null)
                {
                    listener.Dispose();
                    listener = null;
                }
            }
        }
        
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            AndroidEnvironment.UnhandledExceptionRaiser += HandleAndroidException;

            InitGATrackers();

            SetContentView(Resource.Layout.main);            

            if (FragmentManager.BackStackEntryCount == 0)
            {
                var tag = typeof(ServersFragment).Name;

                _serverFragment = FragmentManager.FindFragmentByTag<ServersFragment>(tag);
                if (_serverFragment == null)
                {
                    _serverFragment = new ServersFragment();
                }

                FragmentHelper.AddFragment(this, _serverFragment, tag);
            }
            ReloadInterstitialAd();
        }
        
        private void InitGATrackers()
        {
            if (!_trackers.ContainsKey(EmTraker.GlobalTracker))
            {
                Tracker tracker = GoogleAnalytics.GetInstance(this)
                .NewTracker(Resource.Xml.ga_global_tracker);

                _trackers.Add(EmTraker.GlobalTracker, tracker);
            }
        }

        private void HandleAndroidException(object sender, RaiseThrowableEventArgs e)
        {
            ReportException(e.Exception.ToString());
        }

        private void ReportException(string exceptionStr)
        {
            var tracker = _trackers[EmTraker.GlobalTracker];
            var exBuilder = new HitBuilders.ExceptionBuilder();
            exBuilder.SetDescription("Unhandled Exception" + exceptionStr);
            exBuilder.SetFatal(true);
            var bui = exBuilder.Build();
            var strBuilder = new StringBuilder();
            var report = new Dictionary<string, string>();
            foreach (var i in bui.Keys)
            {
                report.Add(i.ToString(), bui[i].ToString());
            }
            tracker.Send(report);
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ReportException(e.ExceptionObject.ToString());
        }
        
        private void LoadAds()
        {
#if NON_FREE_VERSION
#else
            _adsLayout = this.FindViewById<AdsLayout>(Resource.Id.adsLayout);
            if (_adsLayout != null && !_adsLayout.IsAdsLoaded)
            {
                _adsLayout.Visibility = Android.Views.ViewStates.Visible;
                _adsLayout.LoadAds();
            }
#endif
        }

        protected override void OnResume()
        {
            base.OnResume();

            AndroidCodecProfile.InitProfile(this);

            bool isPlaying = IsPlaying();
            if (!isPlaying || AdsLayout.SHOW_ADS_WHEN_PLAYING)
            {
                LoadAds();
            }            
        }

        private bool IsPlaying()
        {
            var fragment = FragmentManager.GetBackStackEntryAt(FragmentManager.BackStackEntryCount - 1);
            bool isPlaying = fragment != null && fragment.Name == typeof(PlaybackFragment).Name;
            return isPlaying;
        }

        public async void OnServerSelected(AirVidServer selectedServer)
        {
            ProgressDialog progress = new ProgressDialog(this);
            progress.SetMessage("Loading");
            progress.Show();

            var resources = await Task.Run(() => selectedServer.GetResources());

            if (killed)
            {
                return;
            }

            var adp = new AirVidResourcesAdapter(this);
            adp.AddRange(resources);

            DisplayMetrics dispMetrics = new DisplayMetrics();
            WindowManager.DefaultDisplay.GetMetrics(dispMetrics);

            var folderFragment = FolderFragmentFactory.GetFolderFragment(adp, dispMetrics);
            var transaction = FragmentManager.BeginTransaction();

            transaction.Replace(Resource.Id.fragmentPlaceholder, folderFragment);
            transaction.AddToBackStack(null);
            transaction.Commit();
            progress.Dismiss();
        }

        public async void OnFolderSelected(Folder folder)
        {
            ProgressDialog progress = new ProgressDialog(this);
            progress.SetMessage("Loading");
            progress.Show();

            var resources = await Task.Run(() => folder.GetResources());

            if (killed)
            {
                return;
            }

            var adp = new AirVidResourcesAdapter(this);
            adp.AddRange(resources);

            DisplayMetrics dispMetrics = new DisplayMetrics();
            WindowManager.DefaultDisplay.GetMetrics(dispMetrics);

            var folderFragment = FolderFragmentFactory.GetFolderFragment(adp, dispMetrics);

            var transaction = FragmentManager.BeginTransaction();

            transaction.Replace(Resource.Id.fragmentPlaceholder, folderFragment);
            transaction.AddToBackStack(null);
            transaction.Commit();
            progress.Dismiss();            
        }

        public async void OnMediaSelected(Video video, IMediaDetailDisplayer dtDisp)
        {
            ProgressDialog progress = new ProgressDialog(this);
            progress.SetMessage("Loading");
            progress.Show();

            var mediaInfo = await Task.Run(() => video.GetMediaInfo());

            if (killed)
            {
                return;
            }
            dtDisp.DisplayDetail(video, mediaInfo);

            progress.Dismiss();            
        }

        public override bool DispatchTouchEvent(Android.Views.MotionEvent ev)
        {
            if (ev.Action == Android.Views.MotionEventActions.Up)
            {
                ResetAdsClick();
            }
            return base.DispatchTouchEvent(ev);
        }

        public override void OnBackPressed()
        {
            ResetAdsClick();

            if (FragmentManager.BackStackEntryCount > 1)
            {
                base.OnBackPressed();
            }
            else
            {
                if (exitCounter == 1)
                {
                    this.Finish();
                }
                else
                {
                    exitCounter = 1;                   
                    
                    Timer t = new Timer();
                    t.Interval = 5000;
                    t.Elapsed += (sender, arg) => this.RunOnUiThread(() => exitCounter = 0);
                    t.Start();

                    Toast.MakeText(this, Resource.String.ExitPrompt, ToastLength.Short).Show();
                }
            }
        }

        private void ResetAdsClick()
        {
            if (_adsLayout != null
                    && CurrentFocus != _adsLayout
                    && CurrentFocus != _adsLayout.FocusedChild)
            {
                _adsLayout.ResetAdsClickStatus();
            }
        }
        public void OnPlayVideoWithConv(Video vid, MediaInfo mediaInfo, SubtitleStream sub)
        {
            DoPlayVideo(vid.GetPlayWithConvUrl, 
                vid, mediaInfo,
                sub, AndroidCodecProfile.GetProfile());
        }
        public void OnPlayVideo(Video vid, MediaInfo mediaInfo, SubtitleStream sub)
        {
            DoPlayVideo(vid.GetPlaybackUrl, vid,
                mediaInfo, sub, 
                AndroidCodecProfile.GetProfile());
        }

        private async void DoPlayVideo(Func<MediaInfo, SubtitleStream, ICodecProfile, string> funcGetUrl,
            Video vid,
            MediaInfo mediaInfo,
            SubtitleStream sub,
            ICodecProfile codecProfile)
        {
            ProgressDialog progress = new ProgressDialog(this);
            progress.SetMessage("Loading");
            progress.Show();

            string playbackUrl = await Task.Run(() => funcGetUrl(mediaInfo, sub, codecProfile));

            if (killed)
            {
                return;
            }            

            var tag = typeof(PlaybackFragment).Name;

            var playbackFragment = FragmentManager.FindFragmentByTag<PlaybackFragment>(tag);
            if (playbackFragment == null)
            {
                playbackFragment = new PlaybackFragment(playbackUrl, vid.Id, mediaInfo);
            }
            else
            {
                playbackFragment.SetPlaybackSource(playbackUrl, vid.Id, mediaInfo);
            }
            
            var transaction = FragmentManager.BeginTransaction();

            transaction.Replace(Resource.Id.fragmentPlaceholder, playbackFragment, tag);
            transaction.AddToBackStack(tag);
            transaction.Commit();
            progress.Dismiss();
        }        

        public void OnVideoNotPlayable()
        {
            this.OnBackPressed();
            Toast.MakeText(this, Resource.String.CannotPlay, ToastLength.Short).Show();
            LoadAds();
        }

        public void OnVideoFinished(int playedMinutes)
        {
            var adsCriterionMinutes = 10;
#if DEBUG
            adsCriterionMinutes = -1;
#endif
            if (playedMinutes > adsCriterionMinutes)
            {
                if (_fullScreenAds != null && _fullScreenAds.IsLoaded)
                {
                    _fullScreenAds.Show();
                }
            }

            FragmentManager.PopBackStack(typeof(PlaybackFragment).Name, PopBackStackFlags.Inclusive);
            LoadAds();
        }

        public void ReloadInterstitialAd()
        {
#if NON_FREE_VERSION
#else
            ResetFullScreenAds();
            _fullScreenAds = new InterstitialAd(this);
            _fullScreenAds.AdUnitId = "ca-app-pub-3312616311449672/4527954348";

            var adRequest = new AdRequest.Builder()
                .AddTestDevice("421746E519013F2F4FF3B62742A642D1")
                .AddTestDevice("61B125201311D25A92623D5862F94D9A")
                .Build();
            
            _fullScreenAds.AdListener = new InterstitialAdImpl(_fullScreenAds);
            
            _fullScreenAds.LoadAd(adRequest);
#endif
        }
    }

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
            await Task.Delay(5000);
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


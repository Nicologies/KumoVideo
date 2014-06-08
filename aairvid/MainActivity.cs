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

namespace aairvid
{
    [Activity(Label = "aairvid", MainLauncher = false, Icon = "@drawable/icon", NoHistory = false)]
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
            base.OnDestroy();
        }
        
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            AndroidEnvironment.UnhandledExceptionRaiser += HandleAndroidException;

            InitGATrackers();

            SetContentView(Resource.Layout.main);

            if (_fullScreenAds == null)
            {
#if NON_FREE_VERSION
#else
                _fullScreenAds = new InterstitialAd(this);
                _fullScreenAds.AdUnitId = "ca-app-pub-3312616311449672/4527954348";
                var adRequest = new AdRequest.Builder().Build();
                _fullScreenAds.LoadAd(adRequest);
#endif
            }

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

            CodecProfile.InitProfile(this);

            if (!CheckWifiState())
            {
                return;
            }
            var fragment = FragmentManager.GetBackStackEntryAt(FragmentManager.BackStackEntryCount - 1);
            bool isPlaying = fragment != null && fragment.Name == typeof(PlaybackFragment).Name;
            if (!isPlaying)
            {
                LoadAds();
            }            
        }
        private AlertDialog _wifiAlertDialog;
        private bool CheckWifiState()
        {
            var connectivityManager = (ConnectivityManager)GetSystemService(ConnectivityService);
            var wifiState = connectivityManager.GetNetworkInfo(ConnectivityType.Wifi).GetState();
            if (wifiState != NetworkInfo.State.Connected)
            {
                if (_wifiAlertDialog == null)
                {
                    AlertDialog.Builder alertDialogBuilder = new AlertDialog.Builder(this);

                    // Setting Dialog Title
                    alertDialogBuilder.SetTitle("Wifi Required");

                    // Setting Dialog Message
                    alertDialogBuilder
                            .SetMessage("Wifi is not connected.");

                    // On pressing Settings button
                    alertDialogBuilder.SetPositiveButton("OK", (sender, arg) =>
                    {
                        Intent intent = new Intent(
                                Settings.ActionWifiSettings);
                        StartActivity(intent);
                    });

                    _wifiAlertDialog = alertDialogBuilder.Create();
                }

                if (!_wifiAlertDialog.IsShowing)
                {
                    _wifiAlertDialog.Show();
                }
                return false;
            }
            return true;
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

            var folderFragment = FolderFragmentFactory.GetFolderFragment(adp, Resources.Configuration.ScreenLayout);
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

            var folderFragment = FolderFragmentFactory.GetFolderFragment(adp, Resources.Configuration.ScreenLayout);

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
        public void OnPlayVideoWithConv(Video vid, SubtitleStream sub)
        {
            DoPlayVideo(vid.GetPlayWithConvUrl, vid, sub);
        }
        public void OnPlayVideo(Video vid, SubtitleStream sub)
        {
            DoPlayVideo(vid.GetPlaybackUrl, vid, sub);
        }

        private async void DoPlayVideo(Func<SubtitleStream, string> funcGetUrl, Video vid, SubtitleStream sub)
        {
            ProgressDialog progress = new ProgressDialog(this);
            progress.SetMessage("Loading");
            progress.Show();

            string mediaInfo = await Task.Run(() => funcGetUrl(sub));

            if (killed)
            {
                return;
            }

            var tag = typeof(PlaybackFragment).Name;

            var playbackFragment = FragmentManager.FindFragmentByTag<PlaybackFragment>(tag);
            if (playbackFragment == null)
            {
                playbackFragment = new PlaybackFragment(mediaInfo, vid.Id);
            }
            else
            {
                playbackFragment.SetPlaybackSource(mediaInfo, vid.Id);
            }

            var transaction = FragmentManager.BeginTransaction();

            transaction.Replace(Resource.Id.fragmentPlaceholder, playbackFragment, tag);
            transaction.AddToBackStack(tag);
            transaction.Commit();
            progress.Dismiss();
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
        }

        public void OnVideoNotPlayable()
        {
            this.OnBackPressed();
            Toast.MakeText(this, Resource.String.CannotPlay, ToastLength.Short).Show();
            LoadAds();
        }

        public void OnVideoFinished(int playedMinutes)
        {
            if (playedMinutes > 5)
            {
                if (_fullScreenAds != null && _fullScreenAds.IsLoaded)
                {
                    _fullScreenAds.Show();
                }
            }
            LoadAds();
        }
    }
}


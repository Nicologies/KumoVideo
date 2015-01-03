using aairvid.Adapter;
using aairvid.Ads;
using aairvid.Fragments;
using aairvid.Utils;
using Android.App;
using Android.Gms.Ads;
using Android.Gms.Analytics;
using Android.OS;
using Android.Runtime;
using Android.Text.Method;
using Android.Util;
using Android.Widget;
using libairvidproto;
using libairvidproto.model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using ServerContainer = System.Collections.Generic.Dictionary<string, aairvid.ServerAndFolder.CachedServerItem>;

namespace aairvid
{
    [Activity(Label = "aairvid"
        , MainLauncher = false
        , Icon = "@drawable/icon"
        , NoHistory = false
        , ScreenOrientation = Android.Content.PM.ScreenOrientation.SensorLandscape
        , ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize
        )]
	public class MainActivity : Activity, IResourceSelectedListener,
        IPlayVideoListener, IServerSelectedListener, 
        IVideoNotPlayableListener
    {
        private static ServerContainer _cachedServers = new ServerContainer();
        private static readonly string SERVER_PWD_FILE_NAME = "./servers.bin";
        private static readonly string SERVER_PWD_FILE = Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),
            SERVER_PWD_FILE_NAME);
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

            using (var stream = File.OpenWrite(SERVER_PWD_FILE))
            {
                new BinaryFormatter().Serialize(stream, _cachedServers);
            }

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

        public MainActivity()
        {
            LibIniter.Init(new ByteOrderConvAdp(), new SHA1Calculator());
            HistoryMaiten.Load();
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            if (_cachedServers == null || _cachedServers.Count() == 0)
            {
                if (File.Exists(SERVER_PWD_FILE))
                {
                    using (var stream = File.OpenRead(SERVER_PWD_FILE))
                    {
                        var fmt = new BinaryFormatter();
                        _cachedServers = fmt.Deserialize(stream) as ServerContainer;
                    }
                }
            }

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
                    _serverFragment = new ServersFragment(_cachedServers);
                }
                else
                {
                    _serverFragment.SetServers(_cachedServers);
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

            ServerAndFolder.CachedServerItem pwd;
            if (_cachedServers.TryGetValue(selectedServer.ID, out pwd))
            {
                selectedServer.SetPassword(pwd.ServerPassword);
            }
            else
            {
                _cachedServers.Add(selectedServer.ID, new ServerAndFolder.CachedServerItem(selectedServer.Server));
            }

            try
            {
                var resources = await Task <List<AirVidResource>>.Run(() =>
                {
                    try
                    {
                        return selectedServer.GetResources(new WebClientAdp());
                    }
                    catch (System.Net.WebException ex)
                    {
                        ShowConnectionFailure();
                        return new List<AirVidResource>();
                    }
                });

                if (resources.Count() == 0)
                {
                    progress.Dismiss();
                    return;
                }

                if (killed)
                {
                    return;
                }
                if (resources.Count == 1 && resources[0] is Folder)
                {
                    progress.Dismiss();
                    OnFolderSelected(resources[0] as Folder);
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
            catch (InvalidPasswordException)
            {
                progress.Dismiss();

                EditText passwdView = new EditText(this);
                passwdView.InputType = Android.Text.InputTypes.ClassText | Android.Text.InputTypes.TextVariationPassword;
                passwdView.TransformationMethod = PasswordTransformationMethod.Instance;
                passwdView.SetMaxLines(1);

                passwdView.LayoutParameters = new Android.Views.ViewGroup.LayoutParams(-1, 22);
                AlertDialog passwordDlg = new AlertDialog.Builder(this)
                    .SetTitle(Resource.String.PasswordRequired)
                    .SetCancelable(true)
                    .SetView(passwdView)
                    .SetPositiveButton(Android.Resource.String.Ok, delegate
                    {
                        var passwd = passwdView.Text;
                        OnPasswordInputed(selectedServer, passwd);
                    })
                    .Create();

                passwdView.KeyPress += (v, k) => passwdView_KeyPress(v, k, selectedServer, passwordDlg);

                passwordDlg.Show();
            }
        }

        private void ShowConnectionFailure()
        {
            RunOnUiThread(() => Toast.MakeText(this, Resource.String.CannotConnect, ToastLength.Short).Show());
        }

        private void OnPasswordInputed(AirVidServer selectedServer, string passwd)
        {
            selectedServer.SetPassword(passwd);

            _cachedServers[selectedServer.ID] = new ServerAndFolder.CachedServerItem(selectedServer.Server)
            {
                ServerPassword = passwd,
                LastUsedTime = DateTime.Now
            };            

            OnServerSelected(selectedServer);
        }

        void passwdView_KeyPress(object sender, Android.Views.View.KeyEventArgs e, AirVidServer server, AlertDialog dlg)
        {
            e.Handled = false;
            if (e.KeyCode == Android.Views.Keycode.Enter && e.Event.Action == Android.Views.KeyEventActions.Down)
            {
                e.Handled = true;

                var passwd = (sender as EditText).Text;
                OnPasswordInputed(server, passwd);
                dlg.Dismiss();
            }
        }

        public async void OnFolderSelected(Folder folder)
        {
            ProgressDialog progress = new ProgressDialog(this);
            progress.SetMessage("Loading");
            progress.Show();

            var resources = await Task<List<AirVidResource>>.Run(() =>
            {
                try
                {
                    return folder.GetResources(new WebClientAdp()); 
                }
                catch (System.Net.WebException ex)
                {
                    ShowConnectionFailure();
                    return new List<AirVidResource>();
                }                
            });

            if (resources.Count == 0)
            {
                progress.Dismiss();
                return;
            }

            if (killed)
            {
                return;
            }

            if (resources.Count == 1 && resources[0] is Folder)
            {
                progress.Dismiss();
                OnFolderSelected(resources[0] as Folder);
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

            if (resources.Count == 1 && resources[0] is Video)
            {
                OnMediaSelected(resources[0] as Video, folderFragment);
            }
        }

        public async void OnMediaSelected(Video video, IMediaDetailDisplayer dtDisp)
        {
            ProgressDialog progress = new ProgressDialog(this);
            progress.SetMessage("Loading");
            progress.Show();

            var mediaInfo = await Task <MediaInfo>.Run(() =>
            {
                try
                {
                    return video.GetMediaInfo(new WebClientAdp());
                }
                catch (System.Net.WebException ex)
                {
                    ShowConnectionFailure();
                    return null;
                }                
            });

            if (mediaInfo == null)
            {
                progress.Dismiss();
                return;
            }

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
        public void OnPlayVideoWithConv(Video vid, MediaInfo mediaInfo, SubtitleStream sub, AudioStream audio)
        {
            DoPlayVideo(vid.GetPlayWithConvUrl,
                vid, mediaInfo,
                sub, audio, AndroidCodecProfile.GetProfile());
        }
        public void OnPlayVideo(Video vid, MediaInfo mediaInfo, SubtitleStream sub, AudioStream audio)
        {
            DoPlayVideo(vid.GetPlaybackUrl, vid,
                mediaInfo, sub, audio,
                AndroidCodecProfile.GetProfile());
        }

        private async void DoPlayVideo(Func<IWebClient, MediaInfo, SubtitleStream, AudioStream, ICodecProfile, string> funcGetUrl,
            Video vid,
            MediaInfo mediaInfo,
            SubtitleStream sub, AudioStream audio,
            ICodecProfile codecProfile)
        {
            ProgressDialog progress = new ProgressDialog(this);
            progress.SetMessage("Loading");
            progress.Show();

            string playbackUrl = await Task<string>.Run(() =>
            {
                try
                {
                    return funcGetUrl(new WebClientAdp(), mediaInfo, sub, audio, codecProfile);
                }
                catch (System.Net.WebException ex)
                {
                    ShowConnectionFailure();
                    return null;
                }    
            });

            if (string.IsNullOrEmpty(playbackUrl))
            {
                progress.Dismiss();
                return;
            }

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
            var adsCriterionMinutes = 5;
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
                .AddTestDevice("45A39B3DBAE829B7AB8BA9B2C9E55D6F")
                .AddTestDevice("61B125201311D25A92623D5862F94D9A")
                .Build();

            _fullScreenAds.AdListener = new InterstitialAdImpl(_fullScreenAds);

            _fullScreenAds.LoadAd(adRequest);
#endif
        }

		public void PopupSettingsFragment ()
		{
            if (!IsShowingSettings())
            {
                var tag = typeof(SettingsFragment).Name;
                var settingsFragment = FragmentManager.FindFragmentByTag(tag);
                if (settingsFragment == null)
                {
                    settingsFragment = new SettingsFragment();
                }
                FragmentHelper.AddFragment(this, settingsFragment, tag);
            }
		}

        public override bool OnCreateOptionsMenu(Android.Views.IMenu menu)
        {
            this.MenuInflater.Inflate(Resource.Menu.mainmenu, menu);
            return base.OnCreateOptionsMenu(menu);
        }
        public override bool OnOptionsItemSelected(Android.Views.IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.settings:
                    {
                        PopupSettingsFragment();
                        return true;
                    }
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        private bool IsShowingSettings()
        {
            var fragment = FragmentManager.GetBackStackEntryAt(FragmentManager.BackStackEntryCount - 1);
            return fragment != null && fragment.Name == typeof(SettingsFragment).Name;
        }
    }    
}


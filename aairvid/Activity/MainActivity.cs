using aairvid.Adapter;
using aairvid.Ads;
using aairvid.Fragments;
using aairvid.History;
using aairvid.UIUtils;
using aairvid.Utils;
using Android.App;
using Android.Gms.Ads;
using Android.OS;
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
using aairvid.ServerAndFolder;
using aairvid.Settings;
using Android.Content;
using Android.Content.Res;
using Android.Preferences;
using Com.Google.Ads.Consent;
using Java.Net;
using ServerContainer = System.Collections.Generic.Dictionary<string, aairvid.ServerAndFolder.CachedServerItem>;

namespace aairvid
{
    [Activity(Label = "Kumo Video"
        , MainLauncher = false
        , Icon = "@drawable/icon"
        , NoHistory = false
        , ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize
        )]
	public class MainActivity : Activity, IResourceSelectedListener,
        IPlayVideoListener, IServerSelectedListener, 
        IVideoNotPlayableListener, ISupportAdsAndConsent
    {
        private static ServerContainer _cachedServers = new ServerContainer();
        private static readonly string SERVER_PWD_FILE_NAME = "./servers.bin";
        private static readonly string SERVER_PWD_FILE = Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal),
            SERVER_PWD_FILE_NAME);
        ServersFragment _serverFragment;

        private bool _killed = false;

        private int _exitCounter = 0;

        private AdsLayout _adsLayout;

        private InterstitialAd _fullScreenAds;

        protected override void OnDestroy()
        {
            _killed = true;

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

            if (_cachedServers == null || !_cachedServers.Any())
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
            LoadAds();
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
                var resources = await Task.Run(() =>
                {
                    try
                    {
                        return selectedServer.GetServerRootResources(new WebClientAdp());
                    }
                    catch (System.Net.WebException)
                    {
                        ShowConnectionFailure();
                        return new List<AirVidResource>();
                    }
                });

                if (!resources.Any())
                {
                    progress.Dismiss();
                    return;
                }

                if (_killed)
                {
                    return;
                }
                if (resources.Count == 1 && resources[0] is Folder)
                {
                    progress.Dismiss();
                    OnFolderSelected((Folder) resources[0]);
                    return;
                }

                var adp = new AirVidResourcesAdapter(this);
                adp.AddRange(resources);

                var dispMetrics = new DisplayMetrics();
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

                var passwdView = new EditText(this)
                {
                    InputType = Android.Text.InputTypes.ClassText | Android.Text.InputTypes.TextVariationPassword,
                    TransformationMethod = PasswordTransformationMethod.Instance
                };
                passwdView.SetMaxLines(1);

                passwdView.LayoutParameters = new Android.Views.ViewGroup.LayoutParams(-1, 22);
                var passwordDlg = new AlertDialog.Builder(this)
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

            _cachedServers[selectedServer.ID] = new CachedServerItem(selectedServer.Server)
            {
                ServerPassword = passwd,
                LastUsedTime = DateTime.Now
            };            

            OnServerSelected(selectedServer);
        }

        void passwdView_KeyPress(object sender, Android.Views.View.KeyEventArgs e, AirVidServer server, AlertDialog dlg)
        {
            e.Handled = false;
            if (e.KeyCode != Android.Views.Keycode.Enter ||
                e.Event.Action != Android.Views.KeyEventActions.Down) return;
            e.Handled = true;

            var passwd = (sender as EditText)?.Text;
            OnPasswordInputed(server, passwd);
            dlg.Dismiss();
        }

        public async void OnFolderSelected(Folder folder)
        {
            var progress = new ProgressDialog(this);
            progress.SetMessage("Loading");
            progress.Show();

            var resources = await Task.Run(() =>
            {
                try
                {
                    return folder.GetResources(new WebClientAdp()); 
                }
                catch (System.Net.WebException)
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

            if (_killed)
            {
                return;
            }

            if (resources.Count == 1 && resources[0] is Folder)
            {
                progress.Dismiss();
                OnFolderSelected((Folder) resources[0]);
                return;
            }

            var adp = new AirVidResourcesAdapter(this);
            adp.AddRange(resources);

            var dispMetrics = new DisplayMetrics();
            WindowManager.DefaultDisplay.GetMetrics(dispMetrics);

            var folderFragment = FolderFragmentFactory.GetFolderFragment(adp, dispMetrics);

            var transaction = FragmentManager.BeginTransaction();

            transaction.Replace(Resource.Id.fragmentPlaceholder, folderFragment);
            transaction.AddToBackStack(null);
            transaction.Commit();
            progress.Dismiss();

            if (resources.Count == 1 && resources[0] is Video)
            {
                OnMediaSelected((Video) resources[0], folderFragment);
            }
            else if (resources[0] is Video video && ScreenProperty.IsLargeScreen(Resources.DisplayMetrics))
            {
                OnMediaSelected(video, folderFragment);
            }
        }

        public async void OnMediaSelected(Video video, IMediaDetailDisplayer dtDisp)
        {
            var progress = new ProgressDialog(this);
            progress.SetMessage("Loading");
            progress.Show();

            var mediaInfo = await Task.Run(() =>
            {
                try
                {
                    return video.GetMediaInfo(new WebClientAdp());
                }
                catch (System.Net.WebException)
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

            if (_killed)
            {
                return;
            }
            dtDisp.DisplayDetail(video, mediaInfo);

            progress.Dismiss();
        }

        public async void ReqDisplayMediaViaMediaFragment(Video video)
        {
            var progress = new ProgressDialog(this);
            progress.SetMessage("Loading");
            progress.Show();

            var mediaInfo = await Task.Run(() =>
            {
                try
                {
                    return video.GetMediaInfo(new WebClientAdp());
                }
                catch (System.Net.WebException)
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

            if (_killed)
            {
                return;
            }

            var tag = typeof(MediaInfoFragment).Name;

            var mediaInfoFragment = FragmentManager.FindFragmentByTag<MediaInfoFragment>(tag);
            if (mediaInfoFragment == null)
            {
                mediaInfoFragment = new MediaInfoFragment(mediaInfo, video);
            }
            else
            {
                mediaInfoFragment.UpdateWithNewDetail(mediaInfo, video);
            }

            FragmentHelper.AddFragment(this, mediaInfoFragment, tag);

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
            if (FragmentManager.BackStackEntryCount > 1)
            {
                ResetAdsClick();
                base.OnBackPressed();
            }
            else
            {
                TryExit();
            }
        }

        private void TryExit()
        {
            ResetAdsClick();
            if (_exitCounter == 1)
            {
                Finish();
            }
            else
            {
                _exitCounter = 1;

                var t = new Timer {Interval = 5000};
                t.Elapsed += (sender, arg) => RunOnUiThread(() => _exitCounter = 0);
                t.Start();

                Toast.MakeText(this, Resource.String.ExitPrompt, ToastLength.Short).Show();
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
            var progress = new ProgressDialog(this);
            progress.SetMessage("Loading");
            progress.Show();

            var playbackUrl = await Task.Run(() =>
            {
                try
                {
                    return funcGetUrl(new WebClientAdp(), mediaInfo, sub, audio, codecProfile);
                }
                catch (System.Net.WebException)
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

            if (_killed)
            {
                return;
            }

            var intent = new Intent(Intent.ActionView);
            intent.SetDataAndType(Android.Net.Uri.Parse(playbackUrl), "video/*");
            HistoryMaiten.SaveLastPos(vid, 0, vid.Parent);
            StartActivity(intent);
            OnVideoFinished(10);
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

            LoadAds();
        }

        private ConsentForm _consentForm;
        public void ReloadInterstitialAd()
        {
#if NON_FREE_VERSION
#else
            var pref = PreferenceManager.GetDefaultSharedPreferences(ApplicationContext);
            if (!pref.IsConsentCollected(Resources))
            {
                ConsentInformation.RequestConsentInfoUpdate();

               _consentForm = _consentForm ?? new ConsentForm.Builder(ApplicationContext, new URL("http://nicologies.tk/privacy"))
                    .WithListener(new ConsentFormListenerImpl(this, pref, Resources))
                    .WithAdFreeOption()
                    .WithNonPersonalizedAdsOption()
                    .WithPersonalizedAdsOption()
                    .Build();
                _consentForm.Load();
                return;
            }

            ResetFullScreenAds();
            _fullScreenAds = new InterstitialAd(this) {AdUnitId = "ca-app-pub-3312616311449672/4527954348"};

            var adRequest = new AdRequest.Builder()
                .AddTestDevice("421746E519013F2F4FF3B62742A642D1")
                .AddTestDevice("45A39B3DBAE829B7AB8BA9B2C9E55D6F")
                .AddTestDevice("61B125201311D25A92623D5862F94D9A")
                .Build();

            _fullScreenAds.AdListener = new InterstitialAdImpl(_fullScreenAds);

            _fullScreenAds.LoadAd(adRequest);
#endif
        }

        public void ShowConsentForm()
        {
            _consentForm?.Show();
        }

        public void PopupSettingsFragment ()
		{
		    if (IsShowingSettingsFragment()) return;
		    var tag = typeof(SettingsFragment).Name;
		    var settingsFragment = FragmentManager.FindFragmentByTag(tag) ?? new SettingsFragment();
		    FragmentHelper.AddFragment(this, settingsFragment, tag);
		}

        public override bool OnCreateOptionsMenu(Android.Views.IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.mainmenu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(Android.Views.IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menu_settings:
                {
                    PopupSettingsFragment();
                    return true;
                }
                case Resource.Id.menu_recentlyViewed:
                {
                    PopupRecentlyViewedFragment();
                    return true;
                }

                case Resource.Id.menu_exit:
                {
                    TryExit();
                    return true;
                }
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        private bool IsShowingSettingsFragment()
        {
            var fragment = FragmentManager.GetBackStackEntryAt(FragmentManager.BackStackEntryCount - 1);
            return fragment != null && fragment.Name == typeof(SettingsFragment).Name;
        }

        private bool IsShowingRecentlyViewFragment()
        {
            var fragment = FragmentManager.GetBackStackEntryAt(FragmentManager.BackStackEntryCount - 1);
            return fragment != null && fragment.Name == typeof(RecentlyViewedFragment).Name;
        }

        private void PopupRecentlyViewedFragment()
        {
            if (IsShowingRecentlyViewFragment()) return;
            var tag = typeof(RecentlyViewedFragment).Name;
            var fragment = FragmentManager.FindFragmentByTag(tag) ?? new RecentlyViewedFragment();
            FragmentHelper.AddFragment(this, fragment, tag);
        }


        public AirVidServer GetServerById(string id)
        {
            return _serverFragment?.GetServerById(id);
        }
    }

    public interface ISupportAdsAndConsent
    {
        void ReloadInterstitialAd();
        void ShowConsentForm();
    }

    class ConsentFormListenerImpl : ConsentFormListener 
    {
        private readonly ISupportAdsAndConsent _adsAndConsent;
        private readonly ISharedPreferences _pref;
        private readonly Resources _resources;

        public ConsentFormListenerImpl(ISupportAdsAndConsent adsAndConsent,
            ISharedPreferences pref, Resources resources)
        {
            _adsAndConsent = adsAndConsent;
            _pref = pref;
            _resources = resources;
        }

        public override void OnConsentFormLoaded()
        {
            _adsAndConsent.ShowConsentForm();
        }

        public override void OnConsentFormClosed(
            ConsentStatus consentStatus, Java.Lang.Boolean userPrefersAdFree)
        {
            // Consent form was closed.
            _pref.SetAdsPreference(_resources, userPrefersAdFree.BooleanValue());
            _adsAndConsent.ReloadInterstitialAd();
        }

        public override void OnConsentFormError(String errorDescription)
        {
            // Consent form error.
        }
    }
}


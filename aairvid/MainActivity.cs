using aairvid.Adapter;
using aairvid.Model;
using aairvid.UIUtils;
using aairvid.Utils;
using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Preferences;
using Android.Provider;
using Android.Widget;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace aairvid
{
    [Activity(Label = "aairvid", MainLauncher = false, Icon = "@drawable/icon", NoHistory = false)]
    public class MainActivity : Activity, IResourceSelectedListener, IPlayVideoListener, IServerSelectedListener, IVideoNotPlayableListener
    {
        ServersFragment _serverFragment;        

        private bool killed = false;

        private int exitCounter = 0;

        private AdsLayout _adsLayout;

        protected override void OnDestroy()
        {
            killed = true;
            base.OnDestroy();
        }
        
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.main);

            if (FragmentManager.BackStackEntryCount == 0)
            {
                var tag = typeof(ServersFragment).Name;

                _serverFragment = FragmentManager.FindFragmentByTag<ServersFragment>(tag);
                if (_serverFragment == null)
                {
                    _serverFragment = new ServersFragment();
                }

                AddFragment(_serverFragment, tag);
            }
        }

        private void AddFragment(Fragment fragment, string tag)
        {
            var transaction = this.FragmentManager.BeginTransaction();
            transaction.Replace(Resource.Id.fragmentPlaceholder, fragment, tag);
            transaction.AddToBackStack(tag);
            transaction.Commit();
        }

        private void LoadAds()
        {
#if NON_FREE_VERSION
#else
            _adsLayout = this.FindViewById<AdsLayout>(Resource.Id.adsLayout);
            _adsLayout.LoadAds();
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

            LoadAds();
        }

        private bool CheckWifiState()
        {
            var connectivityManager = (ConnectivityManager)GetSystemService(ConnectivityService);
            var wifiState = connectivityManager.GetNetworkInfo(ConnectivityType.Wifi).GetState();
            if (wifiState != NetworkInfo.State.Connected)
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

                alertDialogBuilder.Show();
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

            var folderFragment = new FolderFragment(adp);
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

            var folderFragment = new FolderFragment(adp);

            var transaction = FragmentManager.BeginTransaction();

            transaction.Replace(Resource.Id.fragmentPlaceholder, folderFragment);
            transaction.AddToBackStack(null);
            transaction.Commit();
            progress.Dismiss();            
        }

        public async void OnMediaSelected(Video video)
        {
            ProgressDialog progress = new ProgressDialog(this);
            progress.SetMessage("Loading");
            progress.Show();

            var mediaInfo = await Task.Run(() => video.GetMediaInfo());

            if (killed)
            {
                return;
            }

            var tag = typeof(VideoInfoFragment).Name;

            var mediaInfoFragment = FragmentManager.FindFragmentByTag<VideoInfoFragment>(tag);
            if (mediaInfoFragment == null)
            {
                mediaInfoFragment = new VideoInfoFragment(mediaInfo, video);
            }

            AddFragment(mediaInfoFragment, tag);

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

            var playbackFragment = new PlaybackFragment(mediaInfo, vid.Id);

            var transaction = FragmentManager.BeginTransaction();

            transaction.Replace(Resource.Id.fragmentPlaceholder, playbackFragment);
            transaction.AddToBackStack(null);
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
        }
    }
}


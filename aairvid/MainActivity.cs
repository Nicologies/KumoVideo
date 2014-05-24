using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Network.Bonjour;
using System.Threading.Tasks;
using System.Collections.Generic;
using aairvid.Model;
using aairvid.Adapter;
using aairvid.Protocol;
using Android.Media;
using aairvid.Utils;

using Android.Gms.Ads;
using Android.Net;
using Android.Provider;
using System.Timers;

namespace aairvid
{
    [Activity(Label = "aairvid", MainLauncher = false, Icon = "@drawable/icon", NoHistory = false)]
    public class MainActivity : Activity, IResourceSelectedListener, IPlayVideoListener, IServerSelectedListener, IVideoNotPlayableListener
    {
        ServersFragment _serverFragment;        

        private bool killed = false;

        private int exitCounter = 0;

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
#if FREE_VERSION
            var adsLayout = this.FindViewById<LinearLayout>(Resource.Id.adsLayout);
            if (adsLayout.ChildCount == 0)
            {
                var ad = new AdView(this);
                ad.AdSize = AdSize.SmartBanner;
                ad.AdUnitId = "a1517d1d195f6bf";
                ad.Id = Resource.Id.adView;

                var layoutParams = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.FillParent, ViewGroup.LayoutParams.WrapContent);

                ad.LayoutParameters = layoutParams;

                adsLayout.RemoveAllViews();
                adsLayout.AddView(ad);

                AdRequest adRequest = new AdRequest.Builder()
                    .AddTestDevice(AdRequest.DeviceIdEmulator)
                    .Build();
                ad.LoadAd(adRequest);
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

        public override void OnBackPressed()
        {
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
        public void OnPlayVideoWithConv(Video vid)
        {
            DoPlayVideo(vid.GetPlayWithConvUrl);
        }
        public void OnPlayVideo(Video vid)
        {
            DoPlayVideo(vid.GetPlaybackUrl);
        }

        private async void DoPlayVideo(Func<string> funcGetUrl)
        {
            ProgressDialog progress = new ProgressDialog(this);
            progress.SetMessage("Loading");
            progress.Show();

            string mediaInfo = await Task.Run(funcGetUrl);

            if (killed)
            {
                return;
            }

            var tag = typeof(PlaybackFragment).Name;

            var playbackFragment = new PlaybackFragment(mediaInfo);

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


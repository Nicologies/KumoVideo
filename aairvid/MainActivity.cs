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

namespace aairvid
{
    [Activity(Label = "aairvid", MainLauncher = false, Icon = "@drawable/icon", NoHistory = false)]
    public class MainActivity : Activity, IResourceSelectedListener, IPlayVideoListener, IServerSelectedListener
    {
        ServersFragment _serverFragment;        

        bool killed = false;

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
                var transaction = this.FragmentManager.BeginTransaction();
                transaction.Replace(Resource.Id.fragmentPlaceholder, _serverFragment, tag);
                transaction.AddToBackStack(tag);
                transaction.Commit();
            }
        }

        private void LoadAds()
        {
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

        public async void OnServerSelected(AVServer selectedServer)
        {
            ProgressDialog progress = new ProgressDialog(this);
            progress.SetMessage("Loading");
            progress.Show();

            var resources = await Task.Run(() => selectedServer.GetResources());

            if (killed)
            {
                return;
            }

            var adp = new AVResourceAdapter(this);
            adp.AddRange(resources);

            var folderFragment = new FolderFragment(adp);
            var transaction = FragmentManager.BeginTransaction();

            transaction.Replace(Resource.Id.fragmentPlaceholder, folderFragment);
            transaction.AddToBackStack(null);
            transaction.Commit();
            progress.Dismiss();
        }

        public async void OnFolderSelected(AVFolder folder)
        {
            ProgressDialog progress = new ProgressDialog(this);
            progress.SetMessage("Loading");
            progress.Show();

            var resources = await Task.Run(() => folder.GetResources());

            if (killed)
            {
                return;
            }

            var adp = new AVResourceAdapter(this);
            adp.AddRange(resources);

            var folderFragment = new FolderFragment(adp);

            var transaction = FragmentManager.BeginTransaction();

            transaction.Replace(Resource.Id.fragmentPlaceholder, folderFragment);
            transaction.AddToBackStack(null);
            transaction.Commit();
            progress.Dismiss();            
        }

        public async void OnMediaSelected(AVVideo video)
        {
            ProgressDialog progress = new ProgressDialog(this);
            progress.SetMessage("Loading");
            progress.Show();

            var mediaInfo = await Task.Run(() => video.GetMediaInfo());

            if (killed)
            {
                return;
            }

            var tag = typeof(MediaInfoFragment).Name;

            var mediaInfoFragment = new MediaInfoFragment(mediaInfo, video);

            var transaction = FragmentManager.BeginTransaction();

            transaction.Replace(Resource.Id.fragmentPlaceholder, mediaInfoFragment);
            transaction.AddToBackStack(null);
            transaction.Commit();
            progress.Dismiss();
        }

        public void OnPlayVideoWithConv(AVVideo vid)
        {
            DoPlayVideo(vid.GetPlayWithConvUrl);
        }
        public void OnPlayVideo(AVVideo vid)
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
    }
}


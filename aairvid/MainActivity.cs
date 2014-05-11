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

namespace aairvid
{
    [Activity(Label = "aairvid", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity, IResourceSelectedListener, IPlayVideoListener, IServerSelectedListener
    {
        ServersFragment _serverFragment;

        ProgressDialog progressDetectingServer;

        BonjourServiceResolver _serverDetector;
        
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.main);
            CodecProfile.InitProfile(this);

            if (bundle != null)
            {
            }
            else
            {           
                progressDetectingServer = new ProgressDialog(this);
                progressDetectingServer.SetMessage("Detecting Servers...");
                progressDetectingServer.Show();

                _serverFragment = new ServersFragment();

                var transaction = this.FragmentManager.BeginTransaction();

                transaction.Replace(Resource.Id.fragmentPlaceholder, _serverFragment);
                transaction.AddToBackStack(null);
                transaction.Commit();
            }
            if (_serverDetector == null)
            {
                _serverDetector = new BonjourServiceResolver();
                _serverDetector.ServiceFound += new Network.ZeroConf.ObjectEvent<Network.ZeroConf.IService>(OnServiceFound);
                _serverDetector.Resolve("_airvideoserver._tcp.local.");
            }
        }

        private void OnServiceFound(Network.ZeroConf.IService item)
        {
            if (progressDetectingServer != null)
            {
                progressDetectingServer.Dismiss();
            }

            if (_serverFragment != null)
            {
                this.RunOnUiThread(() => _serverFragment.AddServer(item));
            }
        }

        public async void OnServerSelected(AVServer selectedServer)
        {
            ProgressDialog progress = new ProgressDialog(this);
            progress.SetMessage("Loading");
            progress.Show();

            var resources = await Task.Run(() => selectedServer.GetResources());

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


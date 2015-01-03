using aairvid.Ads;
using aairvid.Utils;
using aairvid.VitamioAdapter;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using libairvidproto.model;
using System;
using System.Linq;

namespace aairvid
{
    public class PlaybackFragment : Fragment
    {        
        private string _playbackUrl;
        private string _vidBaseName;
        private MediaInfo _mediaInfo;

        public PlaybackFragment(string playbackUrl, string mediaId, MediaInfo mediaInfo)
        {
            SetPlaybackSource(playbackUrl, mediaId, mediaInfo);
        }

        protected PlaybackFragment(IntPtr javaReference,
            JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public void SetPlaybackSource(string playbackUrl, string mediaId, MediaInfo mediaInfo)
        {
            this._playbackUrl = playbackUrl;
            _vidBaseName = GetVidBasenameFromId(mediaId);
            _mediaInfo = mediaInfo;
        }

        public static string GetVidBasenameFromId(string mediaId)
        {
            return mediaId.Split('\\').LastOrDefault();
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            RetainInstance = true;
            base.OnCreate(savedInstanceState);
        }

        public override void OnPause()
        {
            if (playbackView != null)
            {
                if (playbackView.IsPlaying)
                {
                    HistoryMaiten.SaveLastPos(_vidBaseName, playbackView.CurrentPosition);
                    playbackView.Suspend();
                }
            }
            
            base.OnPause();
        }

        public override void OnDestroyView()
        {
            if (playbackView != null && _vidBaseName != null)
            {
                //SaveLastPos();
            }

            base.OnDestroyView();
        }

        public override void OnAttach(Activity activity)
        {
            base.OnAttach(activity);

            _startPlayTime = DateTime.MaxValue;

            if (!AdsLayout.SHOW_ADS_WHEN_PLAYING)
            {
                var adsLayout = activity.FindViewById<View>(Resource.Id.adsLayout);
                if (adsLayout != null)
                {
                    adsLayout.Visibility = ViewStates.Gone;
                }
            }

            if (activity.ActionBar != null)
            {
                activity.ActionBar.Hide();
            }
        }
        public override void OnDetach()
        {
            base.OnDetach();            
        }

        private bool _failedToPlay = false;

        private DateTime _startPlayTime = DateTime.MaxValue;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.playback_fragment, container, false);

            PowerManager powerManager = (PowerManager)Activity.GetSystemService(Context.PowerService);
            if (!powerManager.IsScreenOn)
            {
                return view;
            }

            Activity.Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);

            playbackView = view.FindViewById<VitamioVideoView>(Resource.Id.playbackView);

            playbackView.SetVideoChroma(IO.Vov.Vitamio.MediaPlayer.VideochromaRgb565);
                        
            playbackView.Error += playbackView_Error;

            playbackView.Completion += playbackView_Completion;            

            StartPlay(view);

            return view;
        }

        public override void OnDestroy()
        {
            if (playbackView != null)
            {
                playbackView.StopPlayback();
                playbackView = null;
            }
            if (Activity.ActionBar != null)
            {
                Activity.ActionBar.Show();
            }

            Activity.Window.ClearFlags(WindowManagerFlags.Fullscreen);
            
            var pow = (PowerManager)Activity.GetSystemService(Context.PowerService);
            if (!pow.IsScreenOn)
            {
                base.OnDestroy();
                return;
            }

            if (!AdsLayout.SHOW_ADS_WHEN_PLAYING)
            {
                var adsLayout = Activity.FindViewById<View>(Resource.Id.adsLayout);
                if (adsLayout != null)
                {
                    adsLayout.Visibility = ViewStates.Visible;
                }
            }

            var listner = Activity as IPlayVideoListener;
            if (listner != null
                && !_failedToPlay)
            {
                listner.OnVideoFinished((int)(DateTime.Now - _startPlayTime).TotalMinutes);
            }

            base.OnDestroy();
        }

        void playbackView_Completion(object sender, EventArgs e)
        {
            var listner = Activity as IPlayVideoListener;
            if (listner != null
                && !_failedToPlay)
            {
                listner.OnVideoFinished((int)(DateTime.Now - _startPlayTime).TotalMinutes);
            }
        }



        void playbackView_Error(object sender, IO.Vov.Vitamio.MediaPlayer.ErrorEventArgs e)
        {
            _failedToPlay = true;
            var listner = Activity as IVideoNotPlayableListener;
            if (listner != null)
            {
                listner.OnVideoNotPlayable();
            }
        }

        VitamioVideoView playbackView;

        public override void OnResume()
        {
            if (playbackView != null)
            {
                playbackView.Resume();
            }
            base.OnResume();
        }
        
        private void StartPlay(View view)
        {
            try
            {
                playbackView.Prepared += playbackView_Prepared;

                playbackView.SetVideoURI(Android.Net.Uri.Parse(this._playbackUrl));
                
                playbackView.RequestFocus();
            }
            catch (Exception ex)
            {
                Toast.MakeText(this.Activity, ex.ToString(), ToastLength.Long);
                throw;
            }
        }

        void playbackView_Prepared(object sender, EventArgs e)
        {
            var args = e as IO.Vov.Vitamio.MediaPlayer.PreparedEventArgs;
            var player = args.P0 as IO.Vov.Vitamio.MediaPlayer;
            player.SetScreenOnWhilePlaying(true);

            playbackView.SetLayoutStretch(0);

            var mediaController = new IO.Vov.Vitamio.Widget.MediaController(this.Activity);
            mediaController.SetAnchorView(playbackView);

            if (Activity.IsWifiEnabled())
            {
                mediaController.SetInstantSeeking(true);
            }

            playbackView.SetMediaController(mediaController);

            mediaController.SetFileName(this._vidBaseName);

            var stream = _mediaInfo.VideoStreams[0];

            var lastPos = HistoryMaiten.GetLastPos(_vidBaseName);

            var duration = _mediaInfo.DurationSeconds * 1000;

            var distanceToEnd = TimeSpan.FromMilliseconds(duration - lastPos).TotalMinutes;
            if (lastPos != 0
                && lastPos < duration
                && distanceToEnd > 3)
            {
                playbackView.SeekTo(lastPos);
            }

            _startPlayTime = DateTime.Now;

            playbackView.Start();
        }
    }
}
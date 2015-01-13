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

namespace aairvid
{
    public class PlaybackFragment : Fragment
    {        
        private string _playbackUrl;
        private Video _video;
        private MediaInfo _mediaInfo;

        public PlaybackFragment(string playbackUrl, Video v, MediaInfo mediaInfo)
        {
            SetPlaybackSource(playbackUrl, v, mediaInfo);
        }

        protected PlaybackFragment(IntPtr javaReference,
            JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public void SetPlaybackSource(string playbackUrl, Video v, MediaInfo mediaInfo)
        {
            _playbackUrl = playbackUrl;
            _video = v;
            _mediaInfo = mediaInfo;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            RetainInstance = true;
            base.OnCreate(savedInstanceState);
        }

        public override void OnPause()
        {
            if (_playbackView != null)
            {
                if (_playbackView.IsPlaying)
                {
                    HistoryMaiten.SaveLastPos(_video, _playbackView.CurrentPosition, _video.Parent);
                    _playbackView.Suspend();
                }
            }
            
            base.OnPause();
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

        private bool _failedToPlay = false;

        private DateTime _startPlayTime = DateTime.MaxValue;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.playback_fragment, container, false);

            var powerManager = (PowerManager)Activity.GetSystemService(Context.PowerService);
            if (!powerManager.IsScreenOn)
            {
                return view;
            }

            Activity.Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);

            _playbackView = view.FindViewById<VitamioVideoView>(Resource.Id.playbackView);

            _playbackView.SetVideoChroma(IO.Vov.Vitamio.MediaPlayer.VideochromaRgb565);
                        
            _playbackView.Error += playbackView_Error;

            _playbackView.Completion += playbackView_Completion;            

            StartPlay();

            return view;
        }

        public override void OnDestroy()
        {
            if (_playbackView != null)
            {
                _playbackView.Error -= playbackView_Error;

                _playbackView.Completion -= playbackView_Completion;

                _playbackView.Prepared -= playbackView_Prepared;

                _playbackView.StopPlayback();
                _playbackView = null;
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

        VitamioVideoView _playbackView;

        public override void OnResume()
        {
            if (_playbackView != null)
            {
                _playbackView.Resume();
            }
            base.OnResume();
        }
        
        private void StartPlay()
        {
            try
            {
                _playbackView.Prepared += playbackView_Prepared;

                _playbackView.SetVideoURI(Android.Net.Uri.Parse(_playbackUrl));
                
                _playbackView.RequestFocus();
            }
            catch (Exception ex)
            {
                Toast.MakeText(Activity, ex.ToString(), ToastLength.Long);
                throw;
            }
        }

        void playbackView_Prepared(object sender, EventArgs e)
        {
            var args = e as IO.Vov.Vitamio.MediaPlayer.PreparedEventArgs;
            if (args != null)
            {
                var player = args.P0;
                player.SetScreenOnWhilePlaying(true);
            }

            _playbackView.SetLayoutStretch(0);

            var mediaController = new IO.Vov.Vitamio.Widget.MediaController(Activity);
            mediaController.SetAnchorView(_playbackView);

            if (Activity.IsWifiEnabled())
            {
                mediaController.SetInstantSeeking(true);
            }

            _playbackView.SetMediaController(mediaController);

            mediaController.SetFileName(_video.GetDispName());

            var lastPos = HistoryMaiten.GetLastPos(_video.Id);

            var duration = _mediaInfo.DurationSeconds * 1000;

            var distanceToEnd = TimeSpan.FromMilliseconds(duration - lastPos).TotalMinutes;
            if (lastPos != 0
                && lastPos < duration
                && distanceToEnd > 3)
            {
                _playbackView.SeekTo(lastPos);
            }

            _startPlayTime = DateTime.Now;

            _playbackView.Start();
        }
    }
}
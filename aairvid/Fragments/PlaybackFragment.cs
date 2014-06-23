using aairvid.Model;
using aairvid.UIUtils;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using libairvidproto.model;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using HistoryContainer = System.Collections.Generic.Dictionary<string, aairvid.Model.HistoryItem>;

namespace aairvid
{
    public class PlaybackFragment : Fragment
    {        
        private string _playbackUrl;
        private string _mediaId;
        private MediaInfo _mediaInfo;

        private static readonly string HISTORY_FILE_NAME = "./history.bin";
        private static readonly string HISTORY_FILE = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), HISTORY_FILE_NAME);

        private static HistoryContainer _history = new HistoryContainer();

        public PlaybackFragment(string playbackUrl, string mediaId, MediaInfo mediaInfo)
        {
            SetPlaybackSource(playbackUrl, mediaId, mediaInfo);

            if(_history.Count() == 0)
            {
                if(File.Exists(HISTORY_FILE))
                {
                    using (var stream = File.OpenRead(HISTORY_FILE))
                    {
                        var fmt = new BinaryFormatter();
                        _history = fmt.Deserialize(stream) as HistoryContainer;
                    }
                }
            }
        }

        public void SetPlaybackSource(string playbackUrl, string mediaId, MediaInfo mediaInfo)
        {
            this._playbackUrl = playbackUrl;
            _mediaId = mediaId.Split('\\').LastOrDefault();
            _mediaInfo = mediaInfo;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            RetainInstance = true;
            base.OnCreate(savedInstanceState);
            int maxHis = 200;

            if (_history.Count() > maxHis)
            {
                _history.OrderBy(r => r.Value.LastPlayDate);
                _history = _history.Skip(_history.Count()/2).ToDictionary(r => r.Key, r => r.Value);
            }
        }

        public override void OnPause()
        {
            if (playbackView != null)
            {
                if (playbackView.IsPlaying)
                {
                    SaveLastPos();
                    playbackView.Suspend();
                }
            }
            
            base.OnPause();
        }

        public override void OnDestroyView()
        {
            if (playbackView != null && _mediaId != null)
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
            
            activity.ActionBar.Hide();
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

            playbackView = view.FindViewById<VideoView>(Resource.Id.playbackView);

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
            Activity.ActionBar.Show();

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

        void playbackView_Error(object sender, Android.Media.MediaPlayer.ErrorEventArgs e)
        {
            _failedToPlay = true;
            var listner = Activity as IVideoNotPlayableListener;
            if (listner != null)
            {
                listner.OnVideoNotPlayable();
            }
        }

        VideoView playbackView;

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
                var mediaController = new MediaController(this.Activity);
                mediaController.SetAnchorView(playbackView);

                playbackView.Prepared += playbackView_Prepared;

                playbackView.SetVideoURI(Android.Net.Uri.Parse(this._playbackUrl));

                playbackView.SetMediaController(mediaController);

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
            var lastPos = GetLastPos();

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

        private void SaveLastPos()
        {
            HistoryItem hisItem;
            if (_history.TryGetValue(_mediaId, out hisItem))
            {
                hisItem.LastPlayDate = DateTime.Now;
                hisItem.LastPosition = playbackView.CurrentPosition;
            }
            else
            {
                hisItem = new HistoryItem()
                {
                    LastPosition = playbackView.CurrentPosition,
                    LastPlayDate = DateTime.Now
                };
                _history.Add(_mediaId, hisItem);
            }

            using (var stream = File.OpenWrite(HISTORY_FILE))
            {
                new BinaryFormatter().Serialize(stream, _history);
            }
        }
        private int GetLastPos()
        {
            HistoryItem hisItem;
            if (_history.TryGetValue(_mediaId, out hisItem))
            {
                hisItem.LastPlayDate = DateTime.Now;
                return hisItem.LastPosition;
            }
            else
            {
                return 0;
            }
        }
    }
}
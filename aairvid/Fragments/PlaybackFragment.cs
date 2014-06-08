using aairvid.Model;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
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

        private static readonly string HISTORY_FILE_NAME = "./history.bin";
        private static readonly string HISTORY_FILE = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), HISTORY_FILE_NAME);

        private static HistoryContainer _history = new HistoryContainer();

        public PlaybackFragment(string playbackUrl, string mediaId)
        {
            SetPlaybackSource(playbackUrl, mediaId);

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

        public void SetPlaybackSource(string playbackUrl, string mediaId)
        {
            this._playbackUrl = playbackUrl;
            _mediaId = mediaId.Split('\\').LastOrDefault();
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
        
        public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);

            Activity.Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            if (playbackView != null)
            {
                _lastPos = playbackView.CurrentPosition;
            }
        }

        public override void OnPause()
        {
            if (playbackView != null)
            {
                _lastPos = playbackView.CurrentPosition;
            }
            base.OnPause();
        }

        public override void OnDestroyView()
        {
            if (playbackView != null && _mediaId != null)
            {
                var pos = playbackView.CurrentPosition;
                if (pos != 0)
                {
                    if (!_history.ContainsKey(_mediaId))
                    {
                        _history.Add(_mediaId, new HistoryItem()
                        {
                            LastPosition = pos,
                            LastPlayDate = DateTime.Now
                        });
                    }
                    else
                    {
                        var histItem = _history[_mediaId];
                        histItem.LastPosition = pos;
                        histItem.LastPlayDate = DateTime.Now;
                    }

                    using (var stream = File.OpenWrite(HISTORY_FILE))
                    {
                        new BinaryFormatter().Serialize(stream, _history);
                    }
                }
            }

            base.OnDestroyView();
        }

        public override void OnAttach(Activity activity)
        {
            base.OnAttach(activity);

            _startPlayTime = DateTime.MaxValue;

            var adsLayout = activity.FindViewById<View>(Resource.Id.adsLayout);
            if (adsLayout != null)
            {
                adsLayout.Visibility = ViewStates.Gone;
            }
            activity.ActionBar.Hide();
        }
        public override void OnDetach()
        {
            Activity.ActionBar.Show();

            var adsLayout = Activity.FindViewById<View>(Resource.Id.adsLayout);
            if (adsLayout != null)
            {
                adsLayout.Visibility = ViewStates.Visible;
            }

            var listner = Activity as IPlayVideoListener;
            if (listner != null
                && !_failedToPlay)
            {
                listner.OnVideoFinished((int)(DateTime.Now - _startPlayTime).TotalMinutes);
            }

            base.OnDetach();            
        }

        private int _lastPos = 0;

        private bool _failedToPlay = false;

        private DateTime _startPlayTime = DateTime.MaxValue;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.playback_fragment, container, false);

            Activity.Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);

            playbackView = view.FindViewById<VideoView>(Resource.Id.playbackView);

            playbackView.Error += playbackView_Error;

            playbackView.Completion += playbackView_Completion;

            StartPlay(view);

            return view;
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
        
        private void StartPlay(View view)
        {
            try
            {
                var mediaController = new MediaController(this.Activity);
                mediaController.SetAnchorView(playbackView);

                HistoryItem hisItem;
                if (_history.TryGetValue(_mediaId, out hisItem))
                {
                    _lastPos = hisItem.LastPosition;
                    hisItem.LastPlayDate = DateTime.Now;
                }

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
            var distanceToEnd = TimeSpan.FromMilliseconds(playbackView.Duration - _lastPos).TotalMinutes;
            if (_lastPos != 0
                && _lastPos < playbackView.Duration
                && distanceToEnd > 3)
            {
                playbackView.SeekTo(_lastPos);
            }

            _startPlayTime = DateTime.Now;

            playbackView.Start();

            _lastPos = 0;
        }
    }
}
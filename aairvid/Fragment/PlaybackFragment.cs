using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

using HistoryContainer = System.Collections.Generic.Dictionary<string, aairvid.Model.HistoryItem>;
using aairvid.Model;
using Android.Preferences;

namespace aairvid
{
    public class PlaybackFragment : Fragment
    {        
        private string _playbackUrl;
        private string _mediaId;

        private static readonly string HISTORY_FILE = "history.bin";

        private static HistoryContainer _history = new HistoryContainer();

        public PlaybackFragment(string playbackUrl, string mediaId)
        {
            this._playbackUrl = playbackUrl;
            _mediaId = mediaId;

            if(_history.Count() == 0)
            {
                
                if(File.Exists(HISTORY_FILE))
                {
                    var fmt = new BinaryFormatter();
                    _history = fmt.Deserialize(File.OpenRead(HISTORY_FILE)) as HistoryContainer;
                }
            }
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            RetainInstance = true;
            base.OnCreate(savedInstanceState);
            int maxHis = 2;

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

        public override void OnDestroyView()
        {
            if (playbackView != null && _mediaId != null)
            {
                var pos = playbackView.CurrentPosition;
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
                    _history[_mediaId].LastPlayDate = DateTime.Now;
                }

                new BinaryFormatter().Serialize(File.OpenWrite(HISTORY_FILE), _history);
            }

            base.OnDestroyView();
        }

        public override void OnAttach(Activity activity)
        {
            base.OnAttach(activity);
            activity.ActionBar.Hide();
            var ads = activity.FindViewById(Resource.Id.adsLayout);
            if (ads != null)
            {
                ads.Visibility = ViewStates.Gone;
            }
        }
        public override void OnDetach()
        {
            this.Activity.ActionBar.Show();
            var ads = Activity.FindViewById(Resource.Id.adsLayout);
            if (ads != null)
            {
                ads.Visibility = ViewStates.Visible;
            }
            base.OnDetach();            
        }

        private int _lastPos = 0;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.playback_fragment, container, false);

            Activity.Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);

            playbackView = view.FindViewById<VideoView>(Resource.Id.playbackView);

            playbackView.Error += playbackView_Error;

            StartPlay(view);

            return view;
        }

        void playbackView_Error(object sender, Android.Media.MediaPlayer.ErrorEventArgs e)
        {
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

                playbackView.SetVideoURI(Android.Net.Uri.Parse(this._playbackUrl));

                playbackView.SetMediaController(mediaController);

                playbackView.RequestFocus();

                HistoryItem hisItem;
                if (_history.TryGetValue(_mediaId, out hisItem))
                {
                    _lastPos = hisItem.LastPosition;
                    hisItem.LastPlayDate = DateTime.Now;
                }

                playbackView.Prepared += playbackView_Prepared;
            }
            catch (Exception ex)
            {
                Toast.MakeText(this.Activity, ex.ToString(), ToastLength.Long);
                throw;
            }
        }

        void playbackView_Prepared(object sender, EventArgs e)
        {
            if (_lastPos != 0)
            {
                playbackView.SeekTo(_lastPos);
            }

            playbackView.Start();

            _lastPos = 0;
        }
    }
}
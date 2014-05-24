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

namespace aairvid
{
    public class PlaybackFragment : Fragment
    {
        private string _playbackUrl;

        //private static readonly string PARCEL_PLAYBACK_URL= "PlaybackFragment.PlaybackUrl";
        //private static readonly string PARCEL_CURR_POS = "PlaybackFragment.CurrentPosition";

        public PlaybackFragment(string playbackUrl)
        {
            // TODO: Complete member initialization
            this._playbackUrl = playbackUrl;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            RetainInstance = true;
            base.OnCreate(savedInstanceState);
            if (savedInstanceState != null)
            {
               // _playbackUrl = savedInstanceState.GetString(PARCEL_PLAYBACK_URL);
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
            //outState.PutString(PARCEL_PLAYBACK_URL, this._playbackUrl);
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
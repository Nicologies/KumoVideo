using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using aairvid.Adapter;
using libairvidproto.model;
using aairvid.Utils;
using Android.Graphics;

namespace aairvid.Fragments
{
    public class MediaInfoFragmentHelper : IDisposable
    {
        private View _view;
        private Fragment _mediaInfoFragment;
        private MediaInfo _mediaInfo;
        private Video _videoInfo;

        public MediaInfoFragmentHelper(Fragment mediaInfoFragment,
            View view, 
            MediaInfo mediaInfo,
            Video videoInfo
            )
        {
            this._mediaInfoFragment = mediaInfoFragment;
            this._view = view;
            this._mediaInfo = mediaInfo;
            this._videoInfo = videoInfo;

            Init();
        }

        private void InitAudioComboBox()
        {
            var cmbAudioStream = _view.FindViewById<Spinner>(Resource.Id.cmbAudioStream);
            var audioStreamadp = new AudioStreamAdapter(_mediaInfoFragment.Activity);
            audioStreamadp.AddRange(_mediaInfo.AudioStreams);
            cmbAudioStream.Adapter = audioStreamadp;
            cmbAudioStream.SetSelection(audioStreamadp.GetDefaultAudioStream());
        }

        public void Dispose()
        {
            this._mediaInfoFragment = null;
            this._view = null;
            this._mediaInfo = null;
            this._videoInfo = null;
        }

        private void Init()
        {
            InitAudioComboBox();
            InitSubtitleComboBox();

            var tvVidName = _view.FindViewById<TextView>(Resource.Id.tvVideoName);
            tvVidName.Text = _videoInfo.Name;

            var tvDuration = _view.FindViewById<TextView>(Resource.Id.tvVideoDuration);
            var duration = TimeSpan.FromSeconds(_mediaInfo.DurationSeconds);
            tvDuration.Text = string.Format("Duration: {0}:{1}:{2}", duration.Hours, duration.Minutes, duration.Seconds);
            var imageBitmap = BitmapFactory.DecodeByteArray(_mediaInfo.Thumbnail, 0, _mediaInfo.Thumbnail.Length);

            var imgThumbnail = _view.FindViewById<ImageView>(Resource.Id.imgVidThumbnail);
            imgThumbnail.SetImageBitmap(imageBitmap);

            var tvVideoSize = _view.FindViewById<TextView>(Resource.Id.tvVideoSize);
            tvVideoSize.Text = "File Size: " + ReadableFileSize(_mediaInfo.FileSize);

            var btnPlay = _view.FindViewById<Button>(Resource.Id.btnPlay);
            btnPlay.Click += btnPlay_Click;

            var btnPlayWithConv = _view.FindViewById<Button>(Resource.Id.btnPlayWithConv);
            btnPlayWithConv.Click += btnPlayWithConv_Click;      

            var profile = AndroidCodecProfile.GetProfile();
            var stream = _mediaInfo.VideoStreams[0];
            var needConv = stream.Height > profile.DeviceHeight || stream.Width > profile.DeviceWidth;
            if (needConv)
            {
                //btnPlay.Visibility = ViewStates.Gone;
            }
        }

        public static string ReadableFileSize(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }

        private void InitSubtitleComboBox()
        {
            var cmbSubtitle = _view.FindViewById<Spinner>(Resource.Id.cmbSubtitle);
            var adp = new SubtitleAdapter(_mediaInfoFragment.Activity);

            var ordered = _mediaInfo.Subtitles
                .OrderByDescending(r => RecentLans.Instance.GetLanWeight(_mediaInfoFragment.Activity, r.Language.Value));
            adp.AddRange(ordered);
            cmbSubtitle.Adapter = adp;
        }

        void btnPlay_Click(object sender, EventArgs e)
        {
            if (_mediaInfoFragment == null)
            {
                return;
            }
            var sub = GetSelectedSub();
            RecentLans.Instance.UpdateRecentLan(this._mediaInfoFragment.Activity, sub);

            var audio = GetSelectedAudioStream();

            var listener = this._mediaInfoFragment.Activity as IPlayVideoListener;
            listener.OnPlayVideo(_videoInfo, _mediaInfo, sub, audio);
        }

        private SubtitleStream GetSelectedSub()
        {
            var cmbSubtitle = _view.FindViewById<Spinner>(Resource.Id.cmbSubtitle);
            var adp = cmbSubtitle.SelectedItem as SubtitleStreamJavaAdp;
            if (adp != null)
            {
                return adp.Subtitle;
            }
            else
            {
                return null;
            }
        }

        private AudioStream GetSelectedAudioStream()
        {
            var cmbAudioStream = _view.FindViewById<Spinner>(Resource.Id.cmbAudioStream);
            var adp = cmbAudioStream.SelectedItem as AudioStreamJavaAdp;
            if (adp != null)
            {
                return adp.Stream;
            }
            else
            {
                return null;
            }
        }

        void btnPlayWithConv_Click(object sender, EventArgs e)
        {
            if (_mediaInfoFragment == null)
            {
                return;
            }
            var sub = GetSelectedSub();
            RecentLans.Instance.UpdateRecentLan(_mediaInfoFragment.Activity, sub);

            var audio = GetSelectedAudioStream();
            var listener = _mediaInfoFragment.Activity as IPlayVideoListener;
            listener.OnPlayVideoWithConv(_videoInfo, _mediaInfo, sub, audio);
        }
    }
}
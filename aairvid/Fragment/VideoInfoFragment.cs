using aairvid.Utils;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using System;

namespace aairvid
{
    public class VideoInfoFragment : Fragment
    {
        private MediaInfo _mediaInfo;
        private Video _videoInfo;

        public VideoInfoFragment(MediaInfo mediaInfo, Video vid)
        {
            this._mediaInfo = mediaInfo;
            _videoInfo = vid;
        }

        public VideoInfoFragment()
        {
        }
        public override void OnCreate(Bundle savedInstanceState)
        {
            this.RetainInstance = true;
            base.OnCreate(savedInstanceState);
        }

        private static string ReadableFileSize(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; //Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.mediainfo_fragment, container, false);

            DoUpdateUI(view);

            return view;
        }

        private void DoUpdateUI(View view)
        {
            var tvVidName = view.FindViewById<TextView>(Resource.Id.tvVideoName);
            tvVidName.Text = _videoInfo.Name;

            var tvDuration = view.FindViewById<TextView>(Resource.Id.tvVideoDuration);
            var duration = TimeSpan.FromSeconds(_mediaInfo.Duration);
            tvDuration.Text = string.Format("Duration: {0}:{1}:{2}", duration.Hours, duration.Minutes, duration.Seconds);
            var imageBitmap = BitmapFactory.DecodeByteArray(_mediaInfo.Thumbnail, 0, _mediaInfo.Thumbnail.Length);

            var imgThumbnail = view.FindViewById<ImageView>(Resource.Id.imgVidThumbnail);
            imgThumbnail.SetImageBitmap(imageBitmap);

            var tvVideoSize = view.FindViewById<TextView>(Resource.Id.tvVideoSize);
            tvVideoSize.Text = "File Size: " + ReadableFileSize(_mediaInfo.FileSize);

            var btnPlay = view.FindViewById<Button>(Resource.Id.btnPlay);
            btnPlay.Click += btnPlay_Click;

            var btnPlayWithConv = view.FindViewById<Button>(Resource.Id.btnPlayWithConv);
            btnPlayWithConv.Click += btnPlayWithConv_Click;

            var profile = CodecProfile.GetProfile();
            var stream = _mediaInfo.VideoStreams[0];
            var needConv = stream.Height > profile.Height || stream.Width > profile.Width;
            if (needConv)
            {
                btnPlay.Visibility = ViewStates.Gone;
            }
        }

        void btnPlay_Click(object sender, EventArgs e)
        {
            var listener = this.Activity as IPlayVideoListener;
            listener.OnPlayVideo(_videoInfo);
        }

        void btnPlayWithConv_Click(object sender, EventArgs e)
        {
            var listener = this.Activity as IPlayVideoListener;
            listener.OnPlayVideoWithConv(_videoInfo);
        }
    }
}
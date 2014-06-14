using aairvid.Adapter;
using aairvid.Model;
using aairvid.UIUtils;
using aairvid.Utils;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using System;
using System.Linq;

namespace aairvid
{
    public class FolderFragment4LargeScreen : FolderFragment
    {
        private Video _videoInfo;
        private MediaInfo _mediaInfo;

        private bool _wasPlaying = false;

        public FolderFragment4LargeScreen()
        {
        }
        public FolderFragment4LargeScreen(AirVidResourcesAdapter adp)
            : base(adp)
        {
        }

        public override void OnResume()
        {
            if (_wasPlaying && _videoInfo != null && _mediaInfo != null)
            {
                DisplayDetail(_videoInfo, _mediaInfo);
            }
            _wasPlaying = false;
            base.OnResume();
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            RetainInstance = true;
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view =  base.OnCreateView(inflater, container, savedInstanceState);
            var btnPlay = view.FindViewById<Button>(Resource.Id.btnPlay);
            btnPlay.Click += btnPlay_Click;

            var btnPlayWithConv = view.FindViewById<Button>(Resource.Id.btnPlayWithConv);
            btnPlayWithConv.Click += btnPlayWithConv_Click;
            return view;
        }
        public override void DisplayDetail(Video videoInfo, MediaInfo mediaInfo)
        {
            _videoInfo = videoInfo;
            _mediaInfo = mediaInfo;
            var view = this.View;

            var imgEmptyMediaInfo = view.FindViewById<ImageView>(Resource.Id.imgEmptyMediaInfo);
            if (imgEmptyMediaInfo != null)
            {
                imgEmptyMediaInfo.Visibility = ViewStates.Gone;
            }

            var dtView = view.FindViewById(Resource.Id.mediaDetailView);
            if (dtView != null)
            {
                dtView.Visibility = ViewStates.Visible;
            }

            var tvVidName = view.FindViewById<TextView>(Resource.Id.tvVideoName);
            tvVidName.Text = _videoInfo.Name;

            var tvDuration = view.FindViewById<TextView>(Resource.Id.tvVideoDuration);
            var duration = TimeSpan.FromSeconds(_mediaInfo.DurationSeconds);
            tvDuration.Text = string.Format("Duration: {0}:{1}:{2}", duration.Hours, duration.Minutes, duration.Seconds);
            var imageBitmap = BitmapFactory.DecodeByteArray(_mediaInfo.Thumbnail, 0, _mediaInfo.Thumbnail.Length);

            var imgThumbnail = view.FindViewById<ImageView>(Resource.Id.imgVidThumbnail);
            imgThumbnail.SetImageBitmap(imageBitmap);

            var tvVideoSize = view.FindViewById<TextView>(Resource.Id.tvVideoSize);
            tvVideoSize.Text = "File Size: " + VideoInfoFragment.ReadableFileSize(_mediaInfo.FileSize);
            
            var profile = CodecProfile.GetProfile();
            var stream = _mediaInfo.VideoStreams[0];
            var needConv = stream.Height > profile.DeviceHeight || stream.Width > profile.DeviceWidth;
            var btnPlay = view.FindViewById<Button>(Resource.Id.btnPlay);
            btnPlay.Visibility = needConv? ViewStates.Gone : ViewStates.Visible;

            var cmbSubtitle = view.FindViewById<Spinner>(Resource.Id.cmbSubtitle);
            var adp = new SubtitleAdapter(Activity);

            var subs = _mediaInfo.Subtitles.OrderByDescending(r => RecentLans.Instance.GetLanWeight(Activity, r.Language.Value));
            adp.AddRange(subs);
            cmbSubtitle.Adapter = adp;
        }

        void btnPlay_Click(object sender, EventArgs e)
        {
            _wasPlaying = true;
            
            var sub = GetSelectedSub();

            RecentLans.Instance.UpdateRecentLan(Activity, sub);

            var listener = this.Activity as IPlayVideoListener;
            listener.OnPlayVideo(_videoInfo, _mediaInfo, sub);
        }

        private SubtitleStream GetSelectedSub()
        {
            var cmbSubtitle = this.View.FindViewById<Spinner>(Resource.Id.cmbSubtitle);
            return cmbSubtitle.SelectedItem as SubtitleStream;
        }

        void btnPlayWithConv_Click(object sender, EventArgs e)
        {
            var sub = GetSelectedSub();
            var listener = this.Activity as IPlayVideoListener;
            listener.OnPlayVideoWithConv(_videoInfo, _mediaInfo, sub);
        }

        protected override Android.Views.View InflateView(LayoutInflater inflater, ViewGroup container)
        {
            View view = inflater.Inflate(Resource.Layout.folder_fragment_large, container, false);
            return view;
        }
    }
}
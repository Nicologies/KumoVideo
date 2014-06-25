using aairvid.Adapter;
using aairvid.Fragments;
using aairvid.Model;
using aairvid.UIUtils;
using aairvid.Utils;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using libairvidproto.model;
using System;
using System.Linq;

namespace aairvid
{
    public class FolderFragment4LargeScreen : FolderFragment
    {
        private Video _videoInfo;
        private MediaInfo _mediaInfo;

        MediaInfoFragmentHelper _mediaInfoDisplayhelper;

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

                if (_mediaInfoDisplayhelper != null)
                {
                    _mediaInfoDisplayhelper.Dispose();
                }
                _mediaInfoDisplayhelper = new MediaInfoFragmentHelper(this, view, _mediaInfo, _videoInfo);
            }
            else
            {
                _mediaInfoDisplayhelper.Dispose();
                _mediaInfoDisplayhelper = null;
            }
        }
        protected override Android.Views.View InflateView(LayoutInflater inflater, ViewGroup container)
        {
            View view = inflater.Inflate(Resource.Layout.folder_fragment_large, container, false);
            return view;
        }
    }
}
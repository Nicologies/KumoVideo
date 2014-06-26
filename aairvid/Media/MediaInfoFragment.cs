using aairvid.Adapter;
using aairvid.Fragments;
using aairvid.Model;
using aairvid.Utils;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using libairvidproto.model;
using System;
using System.Linq;
namespace aairvid
{
    public class MediaInfoFragment : Fragment
    {
        private MediaInfo _mediaInfo;
        private Video _videoInfo;

        MediaInfoFragmentHelper _mediaInfoDisplayhelper;

        public MediaInfoFragment(MediaInfo mediaInfo, Video vid)
        {
            this._mediaInfo = mediaInfo;
            _videoInfo = vid;
        }

        public MediaInfoFragment()
        {
        }
        public override void OnCreate(Bundle savedInstanceState)
        {
            this.RetainInstance = true;
            base.OnCreate(savedInstanceState);
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.mediainfo_fragment, container, false);

            if (_mediaInfoDisplayhelper != null)
            {
                _mediaInfoDisplayhelper.Dispose();
            }
            _mediaInfoDisplayhelper = new MediaInfoFragmentHelper(this, view, _mediaInfo, _videoInfo);

            return view;
        }
    }
}
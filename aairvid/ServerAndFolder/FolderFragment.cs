using aairvid.Adapter;
using aairvid.Model;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using libairvidproto.model;
using System;
using System.Linq;

namespace aairvid
{
    public abstract class FolderFragment : Fragment, IMediaDetailDisplayer
    {
        protected AirVidResourcesAdapter _resources;

        public FolderFragment(AirVidResourcesAdapter adp)
        {
            _resources = adp;
        }

        protected FolderFragment(IntPtr javaReference,
            JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public FolderFragment()
        {
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (_resources == null)
            {
                _resources = new AirVidResourcesAdapter(this.Activity);
            }            
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = InflateView(inflater, container);

            var lvResources = view.FindViewById<ListView>(Resource.Id.lvResources);
            lvResources.FastScrollEnabled = true;
            lvResources.Adapter = _resources;
            lvResources.ItemClick += OnItemClick;
            return view;
        }

        protected abstract Android.Views.View InflateView(LayoutInflater inflater, ViewGroup container);

        void OnItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var listener = this.Activity as IResourceSelectedListener;
            var res = _resources[e.Position];
            if (res is Folder)
            {
                listener.OnFolderSelected(res as Folder);
            }
            else
            {
                listener.OnMediaSelected(res as Video, this);
            }
        }

        public virtual void DisplayDetail(Video vid, MediaInfo mediaInfo)
        {
            var tag = typeof(MediaInfoFragment).Name;

            var mediaInfoFragment = Activity.FragmentManager.FindFragmentByTag<MediaInfoFragment>(tag);
            if (mediaInfoFragment == null)
            {
                mediaInfoFragment = new MediaInfoFragment(mediaInfo, vid);
            }

            FragmentHelper.AddFragment(Activity, mediaInfoFragment, tag);
        }

        public override void OnDestroyView()
        {
            var lvResources = View.FindViewById<ListView>(Resource.Id.lvResources);
            lvResources.ItemClick -= OnItemClick;
            base.OnDestroyView();
        }
    }
}
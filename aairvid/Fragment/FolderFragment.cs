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
using aairvid.Adapter;
using aairvid.Model;

namespace aairvid
{
    public class FolderFragment : Fragment
    {
        private AirVidResourcesAdapter _resources;

        private static readonly string PARCEL_RESOURCES = "FolderFragment.Resources";

        public FolderFragment(AirVidResourcesAdapter adp)
        {
            _resources = adp;
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

            if (savedInstanceState != null)
            {
                _resources.AddRange(savedInstanceState.GetParcelableArrayList(PARCEL_RESOURCES).Cast<AirVidResource>());
            }
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            outState.PutParcelableArrayList(PARCEL_RESOURCES, _resources.GetResources());
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.folder_fragment, container, false);

            var lvResources = view.FindViewById<ListView>(Resource.Id.lvResources);
            lvResources.FastScrollEnabled = true;
            lvResources.Adapter = _resources;
            lvResources.ItemClick += OnItemClick;
            return view;
        }

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
                listener.OnMediaSelected(res as Video);
            }
        }
    }
}
﻿using aairvid.Model;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace aairvid.Adapter
{
    public class AVResourceAdapter : BaseAdapter<AVResource>
    {
        private LayoutInflater _inflater;
        private List<AVResource> _resources = new List<AVResource>();

        public AVResourceAdapter(Context context)
        {
            _inflater = LayoutInflater.From(context);
        }

        public override int Count
        {
            get { return _resources.Count(); }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override Android.Views.View GetView(int position, Android.Views.View convertView, Android.Views.ViewGroup parent)
        {
            if (convertView == null) // otherwise create a new one
            {
                convertView = _inflater.Inflate(Resource.Layout.avresource_item, null);
            }

            var imgViewForIcon = convertView.FindViewById<ImageView>(Resource.Id.imgResourceIcon);

            var resourceName = convertView
                .FindViewById <TextView>(Resource.Id.tvResourceName);

            var item = this[position];
            resourceName.Text = item.Name;
            if (item is AVFolder)
            {
                imgViewForIcon.SetImageResource(Resource.Drawable.folder);
            }
            else
            {
                imgViewForIcon.SetImageResource(Resource.Drawable.video);
            }
            return convertView;
        }

        public void Add(AVResource res)
        {
            this._resources.Add(res);
            this.NotifyDataSetChanged();
        }

        public override AVResource this[int position]
        {
            get
            {
                if (position >= _resources.Count())
                {
                    return null;
                }
                return _resources[position];
            }
        }

        public void AddRange(IEnumerable<AVResource> res)
        {
            this._resources.AddRange(res);
            this.NotifyDataSetChanged();
        }

        public List<IParcelable> GetResources()
        {
            return this._resources.Cast<IParcelable>().ToList();
        }
    }
}

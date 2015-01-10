using aairvid.Model;
using Android.Content;
using Android.Views;
using Android.Widget;
using System.Collections.Generic;

namespace aairvid.History
{
    public class RecentlyItemListAdapter : BaseAdapter
    {
        private readonly LayoutInflater _inflater;

        private class HistoryItemJavaAdapter : Java.Lang.Object
        {
            public string VideoBaseName { get; set; }
            public HistoryItem Details { get; set; }
        }

        private readonly List<HistoryItemJavaAdapter> _items = new List<HistoryItemJavaAdapter>();

        public RecentlyItemListAdapter(Context context)
        {
            _inflater = LayoutInflater.From(context);
        }

        public override int Count
        {
            get { return _items.Count; }
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return _items[position];
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            if (convertView == null) // otherwise create a new one
            {
                convertView = _inflater.Inflate(Resource.Layout.recently_item, null);
            }

            var serverName = convertView
                .FindViewById<TextView>(Resource.Id.tvVideoBaseName);

            var item = _items[position];
            serverName.Text = item.VideoBaseName + " @ " + item.Details.FolderPath;
            return convertView;
        }

        internal void AddItem(string videoBasename, HistoryItem historyItem)
        {
            _items.Add(new HistoryItemJavaAdapter()
            {
                VideoBaseName = videoBasename,
                Details = historyItem
            });
            NotifyDataSetChanged();
        }
    }
}
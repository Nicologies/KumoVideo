using aairvid.Model;
using aairvid.Utils;
using Android.Content;
using Android.Views;
using Android.Widget;
using System.Collections.Generic;

namespace aairvid.History
{
    public class HistoryItemJavaAdapter : Java.Lang.Object
    {
        public HistoryItem Details { get; set; }
    }
    public class RecentlyItemListAdapter : BaseAdapter<HistoryItemJavaAdapter>
    {
        private readonly Context _ctx;

        private readonly List<HistoryItemJavaAdapter> _items = new List<HistoryItemJavaAdapter>();

        public RecentlyItemListAdapter(Context context)
        {
            _ctx = context;
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

        public string GetVideoName(int position)
        {
            return position < _items.Count ? _items[position].Details.VideoName : "";
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            if (convertView == null) // otherwise create a new one
            {
                var inflater = LayoutInflater.From(_ctx);
                convertView = inflater.Inflate(Resource.Layout.recently_item, null);
            }

            var itemDesc = convertView
                .FindViewById<TextView>(Resource.Id.tvVideoBaseName);

            var item = _items[position];
            itemDesc.Text = item.Details.VideoName + " @ " + item.Details.FolderPath;

            return convertView;
        }

        internal void AddItem(string videoBasename, HistoryItem historyItem)
        {
            _items.Add(new HistoryItemJavaAdapter()
            {
                Details = historyItem
            });
            NotifyDataSetChanged();
        }

        internal void RemoveItem(int p)
        {
            var item = _items[p];
            _items.RemoveAt(p);
            HistoryMaiten.HistoryItems.Remove(item.Details.VideoId);
            HistoryMaiten.SaveAllItems();
            NotifyDataSetChanged();
        }

        public override HistoryItemJavaAdapter this[int position]
        {
            get
            {
                return position >= _items.Count ? null : _items[position];
            }
        }
    }
}
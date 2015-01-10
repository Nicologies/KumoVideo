using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using aairvid.Utils;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.IO;

namespace aairvid.History
{
    public class RecentlyViewedFragment : ListFragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            HistoryMaiten.Load();
            var adp = new RecentlyItemListAdapter(inflater.Context);
            foreach (var item in HistoryMaiten.HistoryItems)
            {
                adp.AddItem(item.Key, item.Value);
            }
            ListAdapter = adp;
            
            return base.OnCreateView(inflater, container, savedInstanceState);
        }
    }
}
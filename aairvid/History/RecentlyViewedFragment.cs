using System;
using aairvid.Utils;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using libairvidproto.model;
using System.Linq;

namespace aairvid.History
{
    public class RecentlyViewedFragment : ListFragment
    {
        private RecentlyItemListAdapter _adp;
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            HistoryMaiten.Load();
            _adp = new RecentlyItemListAdapter(inflater.Context);
            foreach (var item in HistoryMaiten.HistoryItems)
            {
                _adp.AddItem(item.Key, item.Value);
            }
            ListAdapter = _adp;
            
            var view = base.OnCreateView(inflater, container, savedInstanceState);
            
            return view;
        }

        public override void OnCreateContextMenu(IContextMenu menu, View v, IContextMenuContextMenuInfo menuInfo)
        {
            var info = (AdapterView.AdapterContextMenuInfo)menuInfo;
            menu.SetHeaderTitle(_adp.GetVideoName(info.Position));
            var menuItems = Resources.GetStringArray(Resource.Array.recentitems_ctx_menu);
            for (var i = 0; i < menuItems.Length; i++)
            {
                menu.Add(Menu.None, i, i, menuItems[i]);
            }
            base.OnCreateContextMenu(menu, v, menuInfo);
        }

        public override bool OnContextItemSelected(IMenuItem item)
        {
            var info = (AdapterView.AdapterContextMenuInfo)item.MenuInfo;

            var selectedItem = item.TitleFormatted.ToString();

            if (selectedItem == Resources.GetString(Resource.String.DeleteFromHistory))
            {
                _adp.RemoveItem(info.Position);
            }
            else if (selectedItem == Resources.GetString(Resource.String.ViewVideoInfo))
            {
                var player = Activity as IResourceSelectedListener;
                var historyItem = _adp[info.Position];
                var server = player.GetServerById(historyItem.Details.ServerId);

                if (server == null)
                {
                    Toast.MakeText(Activity, Resource.String.CannotFindServer, ToastLength.Short).Show();
                    return true;
                }

                var parent = new AirVidResource.NodeInfo()
                {
                    Id = historyItem.Details.FolderId,
                    Path = historyItem.Details.FolderPath,
                    Name = historyItem.Details.FolderPath.Split('\\').LastOrDefault()
                };

                var v = new Video(server,
                    historyItem.Details.VideoName,
                    historyItem.Details.VideoId, parent);

                player.ReqDisplayMediaViaMediaFragment(v);
            }
            else if (selectedItem == Resources.GetString(Resource.String.ViewFolderInfo))
            {
                var player = Activity as IResourceSelectedListener;
                var historyItem = _adp[info.Position];
                var server = player.GetServerById(historyItem.Details.ServerId);

                if (server == null)
                {
                    Toast.MakeText(Activity, Resource.String.CannotFindServer, ToastLength.Short).Show();
                    return true;
                }

                var parentFolderPath = historyItem.Details.FolderPath.Substring(0,
                    historyItem.Details.FolderPath.LastIndexOf("\\", StringComparison.Ordinal));
                var parent = new AirVidResource.NodeInfo()
                {
                    Id = Guid.Empty.ToString(),
                    Path = parentFolderPath,
                    Name = parentFolderPath.Split('\\').LastOrDefault()
                };
                var v = new Folder(server,
                    historyItem.Details.FolderPath.Split('\\').LastOrDefault(),
                    historyItem.Details.FolderId,
                    parent);

                player.OnFolderSelected(v);
            }
            return true;
        }

        public override void OnListItemClick(ListView l, View v, int position, long id)
        {
            RegisterForContextMenu(l);
            l.ShowContextMenuForChild(v);
        }
    }
}
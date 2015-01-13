using aairvid.Model;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using libairvidproto;
using libairvidproto.model;
using Network.ZeroConf;
using System.Collections.Generic;
using System.Linq;

namespace aairvid
{
    public class ServerListAdapter : BaseAdapter<AirVidServer>
    {
        private LayoutInflater _inflater;
        private List<AirVidServer> _servers = new List<AirVidServer>();
        public ServerListAdapter(Context context)
        {
            _inflater = LayoutInflater.From(context);
        }
        
        public override int Count
        {
            get { return _servers.Count(); }
        }
        
        public override long GetItemId(int position)
        {
            return position;
        }

        public override Android.Views.View GetView(int position, Android.Views.View convertView, Android.Views.ViewGroup parent)
        {            
            if (convertView == null) // otherwise create a new one
            {
                convertView = _inflater.Inflate(Resource.Layout.server_item, null);
            }

            var serverName = convertView
                .FindViewById<TextView>(Resource.Id.tvServerName);

            var item = this[position];
            serverName.Text = item.Name + "@" + item.Server.Address + ":" + item.Server.Port;
            return convertView;
        }

        public void AddServer(IService service)
        {
            AddServer(new BonjourServer(service));
        }

        public AirVidServer AddServer(IServer server)
        {
            var svr = new AirVidServer(server);
            if (_servers.FirstOrDefault(r => r.ID == svr.ID) == null)
            {
                _servers.Add(svr);
            }
            NotifyDataSetChanged();
            return svr;
        }

        public bool Exists(IService server)
        {
            var toSearch = new AirVidServer(new BonjourServer(server));
            return _servers.Exists(r => r.ID == toSearch.ID);
        }

        public override AirVidServer this[int position]
        {
            get
            {
                if (position >= _servers.Count())
                {
                    return null;
                }
                return _servers[position];
            }
        }

        internal void AddServer(IEnumerable<AirVidServer> enumerable)
        {
            _servers.AddRange(enumerable);
        }

        internal void Remove(int p)
        {
            _servers.RemoveAt(p);
            NotifyDataSetChanged();
        }

        public AirVidServer GetServerById(string id)
        {
            return _servers.FirstOrDefault(r => r.ID.ToLowerInvariant() == id.ToLowerInvariant());
        }
    }
}

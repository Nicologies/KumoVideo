using aairvid.Model;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Network.ZeroConf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

            TextView serverName;

            serverName = convertView
                .FindViewById<TextView>(Resource.Id.tvServerName);

            var item = this[position];
            serverName.Text = item.Name;
            return convertView;
        }

        public void AddServer(IService server)
        {
            var svr = new AirVidServer(server);
            if (_servers.FirstOrDefault(r => r.Name == svr.Name) == null)
            {
                _servers.Add(svr);
            }
            this.NotifyDataSetChanged();
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

        internal List<IParcelable> GetAllServers()
        {
            return this._servers.Cast<IParcelable>().ToList();
        }

        internal void AddServer(IEnumerable<AirVidServer> enumerable)
        {
            _servers.AddRange(enumerable);
        }
    }
}

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
using Network.ZeroConf;
using aairvid.Model;

namespace aairvid
{
    public class ServersFragment : Fragment
    {
        ServerListAdapter _servers;        

        private static readonly string PARCEL_SERVERS = "ServersFragment.Servers";

        public ServersFragment()
        {
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (_servers == null)
            {
                _servers = new ServerListAdapter(this.Activity);
            }

            if (savedInstanceState != null)
            {                
                this._servers.AddServer(savedInstanceState.GetParcelableArrayList(PARCEL_SERVERS).Cast<AVServer>());
            }
        }

        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            outState.PutParcelableArrayList(PARCEL_SERVERS, _servers.GetAllServers());
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.servers_fragment, container, false);

            var lvServers = view.FindViewById<ListView>(Resource.Id.lvServers);
            lvServers.FastScrollEnabled = true;
            lvServers.Adapter = _servers;
            lvServers.ItemClick += lvServers_ItemClick;
            return view;
        }

        void lvServers_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var listener = this.Activity as IServerSelectedListener;
            if (listener != null)
            {
                listener.OnServerSelected(this._servers[e.Position]);
            }
        }

        internal void AddServer(IService item)
        {
            this._servers.AddServer(item);
        }
    }
}
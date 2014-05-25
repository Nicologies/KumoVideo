using aairvid.Model;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Network.Bonjour;
using Network.ZeroConf;
using System.Linq;

namespace aairvid
{
    public class ServersFragment : Fragment
    {
        ServerListAdapter _servers;

        ProgressDialog progressDetectingServer;

        BonjourServiceResolver _serverDetector;

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
                this._servers.AddServer(savedInstanceState.GetParcelableArrayList(PARCEL_SERVERS).Cast<AirVidServer>());
            }
        }

        private void OnServiceFound(Network.ZeroConf.IService item)
        {
            if (progressDetectingServer != null)
            {
                progressDetectingServer.Dismiss();
            }
            this.Activity.RunOnUiThread(() => this.AddServer(item));
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

            if (_servers == null)
            {
                _servers = new ServerListAdapter(Activity);
            }
            if (_servers.Count == 0)
            {
                progressDetectingServer = new ProgressDialog(Activity);
                progressDetectingServer.SetMessage("Detecting Servers...");
                progressDetectingServer.Show();

                if (_serverDetector == null)
                {
                    _serverDetector = new Network.Bonjour.BonjourServiceResolver();
                    _serverDetector.ServiceFound += new Network.ZeroConf.ObjectEvent<Network.ZeroConf.IService>(OnServiceFound);
                    _serverDetector.Resolve("_airvideoserver._tcp.local.");
                }
            }
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
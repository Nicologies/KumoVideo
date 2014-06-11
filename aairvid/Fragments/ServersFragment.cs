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
            if (Activity != null)
            {
                Activity.RunOnUiThread(() => this.AddServer(item));
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

            var btnRefreshServer = view.FindViewById<ImageButton>(Resource.Id.btnRefreshServer);
            btnRefreshServer.Click += btnRefreshServer_Click;

            if (_servers == null)
            {
                _servers = new ServerListAdapter(Activity);
            }

            if (_serverDetector != null)
            {
                _serverDetector.ServiceFound -= OnServiceFound;
                _serverDetector = null;
            }

            _serverDetector = new Network.Bonjour.BonjourServiceResolver();
            _serverDetector.ServiceFound += OnServiceFound;

            if (progressDetectingServer == null)
            {
                progressDetectingServer = new ProgressDialog(Activity);
                progressDetectingServer.SetMessage("Detecting Servers...");
            }

            if (_servers.Count == 0)
            {
                RefreshServers();
            }
            return view;
        }

        private void RefreshServers()
        {
            if (progressDetectingServer != null && !progressDetectingServer.IsShowing)
            {
                progressDetectingServer.Show();
            }
            _serverDetector.Resolve("_airvideoserver._tcp.local.");
        }

        void btnRefreshServer_Click(object sender, System.EventArgs e)
        {
            if (_serverDetector != null)
            {
                RefreshServers();
            }
        }

        public override void OnDestroyView()
        {
            if (_serverDetector != null)
            {
                _serverDetector.ServiceFound -= OnServiceFound;
            }
            base.OnDestroyView();
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
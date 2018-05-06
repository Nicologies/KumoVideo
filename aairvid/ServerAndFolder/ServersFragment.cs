using System;
using System.Collections.Generic;
using System.Net;
using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using libairvidproto;
using libairvidproto.model;
using Network.Bonjour;
using Network.ZeroConf;

namespace aairvid.ServerAndFolder
{
    public class ServersFragment : Fragment
    {
        private ServerListAdapter _servers;

        private Dictionary<string, CachedServerItem> _cachedServers = null;

        private ProgressDialog _progressDetectingServer;

        private BonjourServiceResolver _serverDetector;

        public ServersFragment()
        {
        }

        public ServersFragment(Dictionary<string, CachedServerItem> cachedServers)
        {
            SetServers(cachedServers);
        }

        protected ServersFragment(IntPtr javaReference,
            JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            SetHasOptionsMenu(true);
            base.OnCreate(savedInstanceState);

            if (_servers == null)
            {
                _servers = new ServerListAdapter(this.Activity);
            }

            if (_cachedServers == null) return;
            foreach (var ser in _cachedServers)
            {
                IServer concretSer;
                if (ser.Value.IsManual)
                {
                    concretSer = new ManualServer.ManualServerBuilder()
                        .SetAddress(ser.Value.Addr)
                        .SetName(ser.Value.Name)
                        .SetPort(ser.Value.Port).Build();
                }
                else
                {
                    concretSer = new BonjourServer(ser.Value.Name, 
                        ser.Value.Addr,
                        ser.Value.Port,
                        ser.Value.ServerPassword);
                }
                this._servers.AddServer(concretSer);
            }
        }

        private void OnServiceFound(Network.ZeroConf.IService item)
        {
            _progressDetectingServer?.Dismiss();
            Activity?.RunOnUiThread(() => AddServer(item));
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.servers_fragment, container, false);

            var lvServers = view.FindViewById<ListView>(Resource.Id.lvServers);
            lvServers.FastScrollEnabled = true;
            lvServers.Adapter = _servers;
            lvServers.ItemClick += LvServers_ItemClick;

            RegisterForContextMenu(lvServers);

            if (_servers == null)
            {
                _servers = new ServerListAdapter(Activity);
            }

            if (_progressDetectingServer == null)
            {
                _progressDetectingServer = new ProgressDialog(Activity);
                _progressDetectingServer.SetMessage("Detecting Servers...");
            }

            Activity.RunOnUiThread(() =>
            {
                if (_servers.Count == 0)
                {
                    RefreshServers();
                }
            });
            return view;
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Menu.serversmenu, menu);
            base.OnCreateOptionsMenu(menu, inflater);
        }

        public override bool OnOptionsItemSelected(Android.Views.IMenuItem item)
        {
            // Handle item selection
            switch (item.ItemId)
            {
                case Resource.Id.detect_servers:
                    {
                        RefreshServers();
                        return true;
                    }
                case Resource.Id.add_server:
                    {
                        var factory = LayoutInflater.From(this.Activity);
                        var view = factory.Inflate(Resource.Layout.server_detail, null);

                        var editServerPort = view.FindViewById<EditText>(Resource.Id.editServerPort);
                        editServerPort.Text = "45631";
                        
                        var addServerDlg = new AlertDialog.Builder(this.Activity)
                            .SetTitle(Resource.String.AddServer)
                            .SetCancelable(true)
                            .SetView(view)
                            .SetPositiveButton(Android.Resource.String.Ok, (IDialogInterfaceOnClickListener)null)
                            .Create();

                        addServerDlg.ShowEvent += (obj, args) =>
                        {
                            addServerDlg.GetButton((int)Android.Content.DialogButtonType.Positive).Click += (se, arg) =>
                            {
                                var editServerName = view.FindViewById<EditText>(Resource.Id.editServerName);
                                var editServerIp = view.FindViewById<EditText>(Resource.Id.editServerIp);
                                var editServerPwd = view.FindViewById<EditText>(Resource.Id.editServerPwd);

                                if (!IPAddress.TryParse(editServerIp.Text, out var ip))
                                {
                                    editServerIp.RequestFocus();
                                    Toast.MakeText(Activity, Resource.String.InvalidIP, ToastLength.Short).Show();
                                    return;
                                }

                                if (!ushort.TryParse(editServerPort.Text, out var port) || port <= 0 || port >= 65535)
                                {
                                    editServerPort.RequestFocus();
                                    Toast.MakeText(Activity, Resource.String.InvalidPort, ToastLength.Short).Show();
                                    return;
                                }
                                if (string.IsNullOrWhiteSpace(editServerName.Text))
                                {
                                    editServerName.RequestFocus();
                                    Toast.MakeText(Activity, Resource.String.EmptyNotAllowed, ToastLength.Short).Show();
                                    return;
                                }
                                var server = new ManualServer.ManualServerBuilder()
                                    .SetName(editServerName.Text)
                                    .SetAddress(editServerIp.Text)
                                    .SetPort(port)
                                    .SetPassword(editServerPwd.Text)
                                    .Build();

                                var addedSvr = _servers.AddServer(server);
                                _cachedServers.Add(addedSvr.ID, new CachedServerItem(addedSvr.Server));

                                addServerDlg.Dismiss();
                            };
                        };

                        addServerDlg.Show();
                        return true;
                    }
                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        public void RefreshServers()
        {
            if (_serverDetector != null)
            {
                _serverDetector.ServiceFound -= this.OnServiceFound;
                _serverDetector = null;
            }

            _serverDetector = new BonjourServiceResolver();
            _serverDetector.ServiceFound += this.OnServiceFound;
            
            var connectivityManager = (ConnectivityManager)Activity.GetSystemService(Context.ConnectivityService);
            var activeNetworkInfo = connectivityManager.ActiveNetworkInfo;
            if (activeNetworkInfo.Type == ConnectivityType.Wifi &&
                activeNetworkInfo.IsConnected)
            {
                if (_progressDetectingServer != null && !_progressDetectingServer.IsShowing)
                {
                    _progressDetectingServer.Show();
                }

                _serverDetector.Resolve("_airvideoserver._tcp.local.");
            }
            else
            {
                Toast.MakeText(Activity, "Wifi is not connected.", ToastLength.Long).Show();
            }
        }

        public override void OnDestroyView()
        {
            var lvServers = View.FindViewById<ListView>(Resource.Id.lvServers);
            lvServers.ItemClick -= LvServers_ItemClick;

            if (_serverDetector != null)
            {
                _serverDetector.ServiceFound -= OnServiceFound;
                _serverDetector = null;
            }
            base.OnDestroyView();
        }

        private void LvServers_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            if (Activity is IServerSelectedListener listener)
            {
                listener.OnServerSelected(_servers[e.Position]);
            }
        }

        internal void AddServer(IService item)
        {
            _servers.AddServer(item);
        }

        internal void SetServers(Dictionary<string, CachedServerItem> cachedServers)
        {
            _cachedServers = cachedServers;
        }

        public override void OnCreateContextMenu(IContextMenu menu, View v, IContextMenuContextMenuInfo menuInfo)
        {
            if (v.Id == Resource.Id.lvServers)
            {
                var info = (AdapterView.AdapterContextMenuInfo)menuInfo;
                menu.SetHeaderTitle(_servers[info.Position].Name);
                var menuItems = Resources.GetStringArray(Resource.Array.server_ctx_menu);
                for (var i = 0; i < menuItems.Length; i++)
                {
                    menu.Add(Menu.None, i, i, menuItems[i]);
                }
            }
            base.OnCreateContextMenu(menu, v, menuInfo);
        }

        public override bool OnContextItemSelected(IMenuItem item)
        {
            var info = (AdapterView.AdapterContextMenuInfo)item.MenuInfo;

            var selectedItem = item.TitleFormatted.ToString();

            if (selectedItem == Resources.GetString(Resource.String.Delete))
            {
                this._servers.Remove(info.Position);
            }
            return true;
        }

        internal AirVidServer GetServerById(string id)
        {
            return _servers.GetServerById(id);
        }
    }
}

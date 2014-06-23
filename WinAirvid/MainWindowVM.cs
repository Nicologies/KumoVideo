using Network.Bonjour;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using WpfCommon;

namespace WinAirvid
{
    public class MainWindowVM : WpfCommon.ViewModelBase
    {
        BonjourServiceResolver _serverDetector = new BonjourServiceResolver();

        private readonly static string USER_APP_DATA_DIR = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private readonly static string THIS_DATA_DIR = Path.Combine(USER_APP_DATA_DIR, "Nicologies", "WinAirvid");

        private static readonly string PlayerPathConfig = Path.Combine(THIS_DATA_DIR, "PlayerPath.txt");

        public MainWindowVM()
        {
            if (!Directory.Exists(THIS_DATA_DIR))
            {
                Directory.CreateDirectory(THIS_DATA_DIR);
            }
            if (File.Exists(PlayerPathConfig))
            {
                _PlayerPath = File.ReadAllText(PlayerPathConfig);
                NotifyPropertyChange(() => PlayerPath);
            }
            Resources = new ObservableCollection<ResourceVM>();
            _busySetter = new BusyingSetter(this);
            RunOnUIThread(DispatcherPriority.Loaded, () => 
            {
                _serverDetector.ServiceFound += _serverDetector_ServiceFound;
                _serverDetector.Resolve("_airvideoserver._tcp.local.");
            });
        }

        void _serverDetector_ServiceFound(Network.ZeroConf.IService item)
        {
            RunOnUIThread(() =>
            {
                if (Resources.SingleOrDefault(r => r.Name == item.Name) == null)
                {
                    Resources.Add(new ServerVM(item, _busySetter));
                }
            });
        }

        private bool _IsProcessing = false;
        public bool IsProcessing
        {
            get
            {
                return _IsProcessing;
            }
            set
            {
                if (_IsProcessing != value)
                {
                    _IsProcessing = value;
                    NotifyPropertyChange(() => IsProcessing);
                }
            }
        }

        private ResourceVM _SelectedItem = null;
        public ResourceVM SelectedItem
        {
            get
            {
                return _SelectedItem;
            }
            set
            {
                if (_SelectedItem != value)
                {
                    _SelectedItem = value;
                    NotifyPropertyChange(() => SelectedItem);
                }
            }
        }

        public ObservableCollection<ResourceVM> Resources
        {
            get;
            set;
        }

        private BusyingSetter _busySetter;

        public ICommand PlayCmd
        {
            get
            {
                return new RelayCommand(p => Play());
            }
        }

        private void Play()
        {
            var vid = (SelectedItem as VideoVM);

            var t = new Task(() =>
            {
                try
                {
                    var url = vid.GetPlaybackURL();
                    Process.Start(PlayerPath, url);
                }
                catch (Exception ex)
                {
                    RunOnUIThread(() => MessageBox.Show(ex.ToString()));
                }
            });
            t.Start();
        }

        private string _PlayerPath;
        public string PlayerPath
        {
            get
            {
                return _PlayerPath;
            }
            set
            {
                if (_PlayerPath != value)
                {
                    _PlayerPath = value;
                    NotifyPropertyChange(() => PlayerPath);
                    if (!string.IsNullOrEmpty(_PlayerPath))
                    {
                        File.WriteAllText(PlayerPathConfig, _PlayerPath);
                    }
                }
            }
        }
    }

    public class BusyingSetter : IBusyingSetter
    {
        private MainWindowVM _vm;
        public BusyingSetter(MainWindowVM vm)
        {
            _vm = vm;
        }
        public void SetBusying(bool isBusying)
        {
            _vm.IsProcessing = isBusying;
        }
    }
}

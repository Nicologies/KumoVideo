using libairvidproto;
using libairvidproto.model;
using Network.ZeroConf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfCommon;

namespace WinAirvid
{
    public class ServerVM : ResourceVM
    {
        private AirVidServer _server;
        private IBusyingSetter _busyingSetter;
        public ServerVM(IService service, IBusyingSetter busyStatus)
        {
            _server = new AirVidServer(new BonjourServer(service));
            _busyingSetter = busyStatus;
        }

        public override string Name
        {
            get
            {
                return _server.Name;
            }
        }

        public override bool IsSelected
        {
            get
            {
                return _IsSelected;
            }
            set
            {
                if (_IsSelected != value)
                {
                    _IsSelected = value;
                    NotifyPropertyChange(() => IsSelected);
                    if (_IsSelected)
                    {
                        RetrieveRootFolder();
                    }
                }
            }
        }

        private void RetrieveRootFolder()
        {
            if (this.IsLoaded)
            {
                return;
            }
            else
            {
                _busyingSetter.SetBusying(true);
                var t = new Task(DoGetResourceFromServer);
                t.Start();
            }
        }

        private void DoGetResourceFromServer()
        {
            var res = _server.GetResources(new WebClientAdp());
            RunOnUIThread(() =>
            {
                Children.Assign(ConvertResToVM(res, _busyingSetter));
                _busyingSetter.SetBusying(false);
                IsLoaded = true;
                IsExpanded = true;
            });
        }

        public static IEnumerable<ResourceVM> ConvertResToVM(List<AirVidResource> res, IBusyingSetter busyingSetter)
        {
            return res.Select(r =>
            {
                if (r is Folder)
                {
                    return new FolderVM(r as Folder, busyingSetter) as ResourceVM;
                }
                else
                {
                    return new VideoVM(r as Video, busyingSetter) as ResourceVM;
                }
            });
        }
    }
}

using libairvidproto;
using libairvidproto.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfCommon;

namespace WinAirvid
{
    public class FolderVM : ResourceVM
    {
        private Folder _folder;
        private IBusyingSetter _busySetter;
        public FolderVM(Folder f, IBusyingSetter busySetter)
        {
            _folder = f;
            _busySetter = busySetter;
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
                    LoadChildren();
                }
            }
        }

        private void LoadChildren()
        {
            if (!IsLoaded)
            {
                var t = new Task(DoGetResourceFromServer);
                _busySetter.SetBusying(true);
                t.Start();
            }
        }

        private void DoGetResourceFromServer()
        {
            var res = _folder.GetResources(new WebClientAdp());
            RunOnUIThread(() =>
            {
                Children.Assign(ServerVM.ConvertResToVM(res, _busySetter));
                _busySetter.SetBusying(false);
                IsLoaded = true;
                IsExpanded = true;
            });
        }

        public override string Name
        {
            get
            {
                return _folder.Name;
            }
        }
    }
}

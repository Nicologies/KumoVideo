using libairvidproto.model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace WinAirvid
{
    public abstract class ResourceVM : WpfCommon.ViewModelBase
    {
        public ResourceVM()
        {
            Children = new ObservableCollection<ResourceVM>();
        }
        public ObservableCollection<ResourceVM> Children
        {
            get;
            set;
        }

        protected bool _IsSelected = false;
        public abstract bool IsSelected
        {
            get;set;
        }

        public abstract string Name
        {
            get;
        }

        private bool _IsLoaded = false;
        public bool IsLoaded
        {
            get
            {
                return _IsLoaded;
            }
            set
            {
                if (_IsLoaded != value)
                {
                    _IsLoaded = value;
                    NotifyPropertyChange(() => IsLoaded);
                }
            }
        }

        private bool _IsExpandable = true;
        public bool IsExpandable
        {
            get
            {
                return _IsExpandable;
            }
            set
            {
                if (_IsExpandable != value)
                {
                    _IsExpandable = value;
                    NotifyPropertyChange(() => IsExpandable);
                }
            }
        }

        private bool _IsExpanded = false;
        public bool IsExpanded
        {
            get
            {
                return _IsExpanded;
            }
            set
            {
                if (_IsExpanded != value)
                {
                    _IsExpanded = value;
                    NotifyPropertyChange(() => IsExpanded);
                }
            }
        }
    }
}

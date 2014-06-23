using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WinAirvid
{
    public class RootVM : ResourceVM
    {
        public RootVM()
        {
            IsExpandable = false;
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
                }
            }
        }

        public override string Name
        {
            get 
            { 
                return "Servers"; 
            }
        }
    }
}

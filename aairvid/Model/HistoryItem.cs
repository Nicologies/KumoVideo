using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace aairvid.Model
{
    [Serializable]
    public class HistoryItem
    {
        public int LastPosition
        {
            get;
            set;
        }
        public DateTime LastPlayDate
        {
            get;
            set;
        }
    }
}

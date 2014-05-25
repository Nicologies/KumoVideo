using System;

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

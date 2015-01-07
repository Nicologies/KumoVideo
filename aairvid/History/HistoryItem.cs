﻿using System;

namespace aairvid.Model
{
    [Serializable]
    public class HistoryItem
    {
        public long LastPosition
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
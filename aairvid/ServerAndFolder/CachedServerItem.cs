using libairvidproto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace aairvid.ServerAndFolder
{
    [Serializable]
    public class CachedServerItem
    {
        protected CachedServerItem()
        { }
        public CachedServerItem(IServer ser)
        {
            IsManual = ser.IsManual;
            Name = ser.Name;
            Addr = ser.Address;
            Port = ser.Port;
        }

        public bool IsManual { get; set; }
        public string Name { get; set; }
        public string Addr { get; set; }
        public ushort Port { get; set; }
        public string ServerPassword { get; set; }
        public DateTime LastUsedTime { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using libairvidproto;
using Network.ZeroConf;

namespace aairvid
{
    public class BonjourServer : IServer
    {
        public BonjourServer(IService service)
        {
            Name = service.Name;
            var addr = service.Addresses[0];
            var str = addr.Addresses[0].ToString();
            Address = str;
            Port = addr.Port;
        }
        public BonjourServer(string name, string addr, ushort port, string pwd)
        {
            Name = name;
            Address = addr;
            Port = port;
            Password = pwd;
        }

        public bool IsManual
        {
            get
            {
                return false;
            }
        }

        public string Name
        {
            get;
            private set;
        }

        public string Address
        {
            get;
            private set;
        }

        public ushort Port
        {
            get;
            private set;
        }

        public string Password
        {
            get;
            private set;
        }
    }
}
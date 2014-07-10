using libairvidproto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace aairvid.ServerAndFolder
{
    public class ManualServer : IServer
    {
        public class ManualServerBuilder
        {
            private ManualServer _server = new ManualServer();
            public ManualServerBuilder SetName(string name)
            {
                _server.Name = name;
                return this;
            }
            public ManualServerBuilder SetAddress(string address)
            {
                _server.Address = address;
                return this;
            }

            public ManualServerBuilder SetPort(ushort port)
            {
                _server.Port = port;
                return this;
            }
            public ManualServerBuilder SetPassword(string password)
            {
                _server.Password = password;
                return this;
            }

            public ManualServer Build()
            {
                return _server;
            }
        }

        public bool IsManual
        {
            get
            {
                return true;
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


using System.Text;
namespace libairvidproto.model
{
    public class AirVidResource
    {
        protected enum EmContentType
        {
            Folder = 1,
            Video = 2,
            MediaInfo = 3
        }

        public AirVidServer Server
        {
            get;
            private set;
        }
        public string Name
        {
            get;
            private set;
        }

        public string Id
        {
            get;
            private set;
        }

        public AirVidResource(AirVidServer server, string name, string id, AirVidResource parent)
        {
            Server = server;
            Name = name;
            Id = id;
            Parent = parent;
        }

        public  AirVidResource Parent { get; private set; }

        public string GetPath()
        {
            var p = Parent;
            var ret = new StringBuilder(Name);
            while (p != null)
            {
                ret.Insert(0, string.Format(@"{0}/", p.Name));
                p = p.Parent;
            }
            return ret.ToString();
        }
    }
}

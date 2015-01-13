
using System.Text;
namespace libairvidproto.model
{
    public class AirVidResource
    {
        public class NodeInfo
        {
            public string Id;
            public string Path;
            public string Name;
        }
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

        public AirVidResource(AirVidServer server, string name, string id, NodeInfo parent)
        {
            Server = server;
            Name = name;
            Id = id;
            Parent = parent;
        }

        public NodeInfo Parent { get; private set; }

        public NodeInfo GetNodeInfo()
        {
            return new NodeInfo()
            {
                Id = this.Id,
                Name = this.Name,
                Path = this.GetPath()
            };
        }

        public string GetPath()
        {
            return Parent != null ? string.Format(@"{0}\{1}", Parent.Path, Name) : "";
        }
    }
}

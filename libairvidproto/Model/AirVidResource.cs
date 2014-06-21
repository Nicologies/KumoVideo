using aairvid.Model;

namespace aairvid
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

        public AirVidResource(AirVidServer server, string name, string id)
        {
            Server = server;
            Name = name;
            Id = id;
        }
    }
}

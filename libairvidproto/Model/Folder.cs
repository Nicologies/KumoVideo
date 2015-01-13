using System.Collections.Generic;

namespace libairvidproto.model
{
    public class Folder : AirVidResource
    {
        public static readonly int ContentType = (int)EmContentType.Folder;
        public Folder(AirVidServer server, string name, string id, NodeInfo parent) : base(server, name, id, parent)
        {
        }

        public List<AirVidResource> GetResources(IWebClient webClient)
        {
            return Server.GetResources(webClient, Id, this);
        }
    }
}

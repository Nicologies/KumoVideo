using libairvidproto;
using System.Collections.Generic;

namespace aairvid.Model
{
    public class Folder : AirVidResource
    {
        public static readonly int ContentType = (int)EmContentType.Folder;
        public Folder(AirVidServer server, string name, string id) : base(server, name, id)
        {
        }

        public List<AirVidResource> GetResources(IWebClient webClient)
        {
            return Server.GetResources(webClient, this.Id.ToString());
        }
    }
}

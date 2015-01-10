
namespace libairvidproto.model
{
    public class RootFolder : Folder
    {
        public RootFolder(AirVidServer server)
            : base(server, server.Name, "/", null)
        {
        }
    }
}

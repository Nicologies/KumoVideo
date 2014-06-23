
using aairvid.Model;
using libairvidproto.model;

namespace aairvid
{
    interface IServerSelectedListener
    {
        void OnServerSelected(AirVidServer server);        
    }
}
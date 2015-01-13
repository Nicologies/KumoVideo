
using aairvid.Model;
using libairvidproto;
using libairvidproto.model;

namespace aairvid
{
    public interface IResourceSelectedListener
    {
        void OnFolderSelected(Folder folder);
        void OnMediaSelected(Video media, IMediaDetailDisplayer disp);
        void ReqDisplayMediaViaMediaFragment(Video video);
        AirVidServer GetServerById(string serverId);
    }
}
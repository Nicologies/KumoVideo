
using aairvid.Model;
using libairvidproto.model;

namespace aairvid
{
    public interface IResourceSelectedListener
    {
        void OnFolderSelected(Folder folder);
        void OnMediaSelected(Video media, IMediaDetailDisplayer disp);
    }
}
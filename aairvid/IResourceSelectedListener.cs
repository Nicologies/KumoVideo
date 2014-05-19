
using aairvid.Model;

namespace aairvid
{
    public interface IResourceSelectedListener
    {
        void OnFolderSelected(Folder folder);
        void OnMediaSelected(Video media);
    }
}
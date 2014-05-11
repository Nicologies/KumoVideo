
using aairvid.Model;

namespace aairvid
{
    public interface IResourceSelectedListener
    {
        void OnFolderSelected(AVFolder folder);
        void OnMediaSelected(AVVideo media);
    }
}
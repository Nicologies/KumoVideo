using Android.OS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aairvid.Model
{
    public class AVFolder : AVResource
    {
        public static readonly int ContentType = (int)EmContentType.Folder;
        public AVFolder(AVServer server, string name, string id) : base(server, name, id)
        {
        }

        public AVFolder(Parcel source) : base(source)
        {
        }
        public List<AVResource> GetResources()
        {
            return Server.GetResources(this.Id.ToString());
        }

        public override int DescribeContents()
        {
            return ContentType;
        }
    }
}

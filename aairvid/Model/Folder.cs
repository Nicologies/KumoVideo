using Android.OS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace aairvid.Model
{
    public class Folder : AirVidResource
    {
        public static readonly int ContentType = (int)EmContentType.Folder;
        public Folder(AirVidServer server, string name, string id) : base(server, name, id)
        {
        }

        public Folder(Parcel source) : base(source)
        {
        }
        public List<AirVidResource> GetResources()
        {
            return Server.GetResources(this.Id.ToString());
        }

        public override int DescribeContents()
        {
            return ContentType;
        }
    }
}

using aairvid.Model;
using aairvid.Protocol;
using Android.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace aairvid
{
    public class Video : AirVidResource
    {
        public static readonly int ContentType = (int)EmContentType.Video;

        public Video(AirVidServer server, string name, string id)
            : base(server, name, id)
        {
        }

        public Video(Android.OS.Parcel source) : base(source)
        {
        }

        public MediaInfo GetMediaInfo()
        {
            return Server.GetMediaInfo(Id);
        }

        internal string GetPlaybackUrl()
        {
            return Server.GetPlaybackUrl(this);
        }
        
        internal string GetPlayWithConvUrl()
        {
            return Server.GetPlayWithConvUrl(this);
        }

        public override int DescribeContents()
        {
            return ContentType;
        }
    }
}

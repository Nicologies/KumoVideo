using aairvid.Model;
using aairvid.Protocol;
using Android.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace aairvid
{
    public class AVVideo : AVResource
    {
        public static readonly int ContentType = (int)EmContentType.Video;

        public AVVideo(AVServer server, string name, string id)
            : base(server, name, id)
        {
        }

        public AVVideo(Android.OS.Parcel source) : base(source)
        {
        }

        public AVMediaInfo GetMediaInfo()
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

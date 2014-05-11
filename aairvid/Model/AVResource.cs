using aairvid.Model;
using Android.OS;
using Java.Interop;
using Network.ZeroConf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace aairvid
{
    public class AVResource : Java.Lang.Object, IParcelable
    {
        protected enum EmContentType
        {
            Folder = 1,
            Video = 2,
            MediaInfo = 3
        }

        public AVServer Server
        {
            get;
            private set;
        }
        public string Name
        {
            get;
            private set;
        }

        public string Id
        {
            get;
            private set;
        }

        public AVResource(AVServer server, string name, string id)
        {
            Server = server;
            Name = name;
            Id = id;
        }

        public AVResource(Parcel source)
        {
            Name = source.ReadString();
            Id = source.ReadString();
            Server = new AVServer(source);
        }

        public virtual int DescribeContents()
        {
            return 0;
        }

        public virtual void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
        {
            dest.WriteInt(DescribeContents());
            dest.WriteString(Name);
            dest.WriteString(Id);
            Server.WriteToParcel(dest, flags);
        }

        [ExportField("CREATOR")]
        public static AVResourceCreator InitializeCreator()
        {
            return new AVResourceCreator();
        }
    }
}

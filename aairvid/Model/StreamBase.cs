﻿using Android.OS;

namespace aairvid.Model
{
    public class StreamBase : Java.Lang.Object, IParcelable
    {
        public int index = 0;
        public int StreamType = 0;
        public string Codec = "";
        public string Language = "";

        public StreamBase()
        {
        }

        public StreamBase(Parcel source)
        {
            index = source.ReadInt();
            StreamType = source.ReadInt();
            Codec = source.ReadString();
            Language = source.ReadString();
        }

        public int DescribeContents()
        {
            return 0;
        }

        public virtual void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
        {
            dest.WriteInt(index);
            dest.WriteInt(StreamType);
            dest.WriteString(Codec);
            dest.WriteString(Language);
        }
    }
}

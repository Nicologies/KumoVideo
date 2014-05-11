using Android.OS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace aairvid.Model
{
    public class AVResourceCreator : Java.Lang.Object, IParcelableCreator
    {
        public Java.Lang.Object CreateFromParcel(Parcel source)
        {
            int description = source.ReadInt();
            if (description == AVFolder.ContentType)
            {
                return new AVFolder(source);
            }
            else
            {
                return new AVVideo(source);
            }
        }

        public Java.Lang.Object[] NewArray(int size)
        {
            return new AVResource[size];
        }
    }
}

using Android.OS;

namespace aairvid.Model
{
    public class AirVidResourceCreator : Java.Lang.Object, IParcelableCreator
    {
        public Java.Lang.Object CreateFromParcel(Parcel source)
        {
            int description = source.ReadInt();
            if (description == Folder.ContentType)
            {
                return new Folder(source);
            }
            else
            {
                return new Video(source);
            }
        }

        public Java.Lang.Object[] NewArray(int size)
        {
            return new AirVidResource[size];
        }
    }
}

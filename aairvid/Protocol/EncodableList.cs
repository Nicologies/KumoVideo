using System.Collections.Generic;

namespace aairvid.Protocol
{
    public class EncodableList : Encodable, IEnumerable<Encodable>
    {
        public EncodableList(List<Encodable> objs)
        {
            AVProtoObjects = objs;
        }

        public EncodableList(Encodable obj) : this()
        {
            AVProtoObjects.Add(obj);
        }

        public EncodableList()
        {
            AVProtoObjects = new List<Encodable>();
        }

        public void Add(Encodable en)
        {
            AVProtoObjects.Add(en);
        }
        public List<Encodable> AVProtoObjects
        {
            get;
            private set;
        }
        public void Encode(Encoder encoder)
        {
            encoder.Encode(AVProtoObjects);
        }

        public IEnumerator<Encodable> GetEnumerator()
        {
            return AVProtoObjects.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return AVProtoObjects.GetEnumerator();
        }
    }
}

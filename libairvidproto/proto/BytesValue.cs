using System;

namespace libairvidproto.types
{
    public class BytesValue : KeyValueBase
    {
        public BytesValue(string key, byte[] bytes) : base(key)
        {
            Value = bytes;
        }

        public byte[] Value
        {
            get;
            private set;
        }

        public override void Encode(Encoder encoder)
        {
            throw new NotImplementedException();
        }
    }
}

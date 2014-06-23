using System;

namespace libairvidproto.types
{
    public class BigIntValue : KeyValueBase
    {
        public BigIntValue(string key, long value) : base(key)
        {
            Value = value;
        }

        public long Value
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

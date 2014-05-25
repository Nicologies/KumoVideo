﻿
namespace aairvid.Protocol
{
    public class StringValue : KeyValueBase
    {
        public StringValue(string key, string v) : base(key)
        {
            Value = v;
        }
        public string Value
        {
            get;
            private set;
        }
        public override void Encode(Encoder encoder)
        {
            encoder.Encode(this);
        }
    }
}

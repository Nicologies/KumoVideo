
namespace aairvid.Protocol
{
    public class StringValue : KeyValueBase
    {
        public StringValue(string key, string v) : base(key)
        {
            Value = v;
        }

        public StringValue(string key, string v, int id)
            : base(key)
        {
            Value = v;
            Id = id;
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

        public int Id
        {
            get;
            set;
        }
    }
}

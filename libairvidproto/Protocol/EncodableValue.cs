
namespace aairvid.Protocol
{
    public class EncodableValue : KeyValueBase
    {
        public EncodableValue(string key) : base(key)
        {
        }

        public EncodableValue(string key, Encodable obj)
            : base(key)
        {
            this.Value = obj;
        }

        public Encodable Value
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

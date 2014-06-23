
namespace libairvidproto.types
{
    public class DeviceInfoValue : KeyValueBase
    {
        public DeviceInfoValue(string key, EncodableList value)
            : base(key)
        {
            Value = value;
        }

        public DeviceInfoValue(string key)
            : this(key, new EncodableList())
        {
        }

        public EncodableList Value
        {
            get;
            private set;
        }

        public override void Encode(Encoder encoder)
        {
            encoder.Encode(this);
        }

        public void Add(Encodable en)
        {
            this.Value.Add(en);
        }
    }
}

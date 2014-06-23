
namespace libairvidproto.types
{
    public class IntValue : KeyValueBase
    {
        public IntValue(string key, int v)
            : base(key)
        { 
            Value = v;
        }
        public override void Encode(Encoder encoder)
        {
            encoder.Encode(this);
        }

        public int Value
        {
            get;
            private set;
        }
    }
}

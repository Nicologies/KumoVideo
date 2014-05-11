using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace aairvid.Protocol
{

    public class DoubleValue : KeyValueBase
    {
        public DoubleValue(string key, double v)
            : base(key)
        {
            Value = v;
        }
        public override void Encode(Encoder encoder)
        {
            encoder.Encode(this);
        }

        public double Value
        {
            get;
            private set;
        }
    }
}

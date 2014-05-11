using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace aairvid.Protocol
{
    public class BitratesValue: KeyValueBase
    {
        public BitratesValue(string key, List<string> bitrates) : base(key)
        {
            Value = bitrates;
        }

        public BitratesValue(string key, string bitrate)
            : this(key, new List<string>(){bitrate})
        {
        }


        public List<string> Value
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace aairvid.Protocol
{
    public interface Encodable
    {
        void Encode(Encoder encoder);
    }
}

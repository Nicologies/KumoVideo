﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace aairvid.Protocol
{
    public abstract class KeyValueBase : Encodable
    {
        public KeyValueBase(string key)
        {
            Key = key;
        }

        public abstract void Encode(Encoder encoder);
        public string Key
        {
            get;
            private set;
        }
    }
}

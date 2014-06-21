using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace aairvid.Protocol
{
    public class Encoder
    {
        public Encoder(BinaryWriter w)
        {
            _writer = w;
        }
        private BinaryWriter _writer;
        private int _fieldCounter = 0;
        public void Encode(StringValue avString)
        {
            string key = avString.Key;
            EncodeKey(key);

            string str = avString.Value;

            if(string.IsNullOrWhiteSpace(str))
            {
                _writer.Write('n');// null
            }
            else
            {
                _writer.Write('s');
                _writer.Write(IPAddress.HostToNetworkOrder(_fieldCounter++));
                var bytes = Encoding.UTF8.GetBytes(str);
                _writer.Write(IPAddress.HostToNetworkOrder(bytes.Length));
                _writer.Write(bytes);
            }
            
        }

        public void Encode(IntValue avInt)
        {
            string key = avInt.Key;
            EncodeKey(key);
            _writer.Write('i');
            _writer.Write(IPAddress.HostToNetworkOrder((int)avInt.Value));
        }

        public void Encode(List<Encodable> avObjs)
        {
            _writer.Write('a'); // array
            _writer.Write(IPAddress.HostToNetworkOrder(_fieldCounter++));
            _writer.Write(IPAddress.HostToNetworkOrder(avObjs.Count()));
            foreach (var obj in avObjs)
            {
                obj.Encode(this);
            }
        }

        private void EncodeKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }
            var keyBytes = Encoding.ASCII.GetBytes(key);
            _writer.Write(IPAddress.HostToNetworkOrder(keyBytes.Length));
            _writer.Write(keyBytes);
        }

        public void Encode(RootObj protoObj)
        {
            _writer.Write('o');
            _writer.Write(IPAddress.HostToNetworkOrder(this._fieldCounter++));

            var objType = Encoding.ASCII.GetBytes(protoObj.ObjType);
            _writer.Write(IPAddress.HostToNetworkOrder((int)objType.Length));
            _writer.Write(objType);

            _writer.Write(IPAddress.HostToNetworkOrder((int)protoObj.MValue));

            _writer.Write(IPAddress.HostToNetworkOrder((int)protoObj.Children.Count()));

            foreach (var c in protoObj.Children)
            {
                c.Encode(this);
            }
        }

        public void Encode(EncodableValue protoObj)
        {
            EncodeKey(protoObj.Key);
            protoObj.Value.Encode(this);
        }

        public void Encode(BitratesValue bitratesValue)
        {
            EncodeKey(bitratesValue.Key);
            _writer.Write('e');
            _writer.Write(IPAddress.HostToNetworkOrder(this._fieldCounter++));
            _writer.Write(IPAddress.HostToNetworkOrder(bitratesValue.Value.Count()));
            foreach (var rate in bitratesValue.Value)
            {
                var str = new StringValue(null, rate);
                str.Encode(this);
            }
        }

        public void Encode(DoubleValue doubleValue)
        {
            this.EncodeKey(doubleValue.Key);

            _writer.Write('f');

            var int64Value = BitConverter.DoubleToInt64Bits(doubleValue.Value);
            
            _writer.Write(IPAddress.HostToNetworkOrder(int64Value));
        }

        public void Encode(DeviceInfoValue deviceInfoValue)
        {
            this.EncodeKey(deviceInfoValue.Key);

            _writer.Write('d');

            _writer.Write(IPAddress.HostToNetworkOrder(this._fieldCounter++));
            _writer.Write(IPAddress.HostToNetworkOrder(deviceInfoValue.Value.Count()/2));

            foreach (var en in deviceInfoValue.Value)
            {
                en.Encode(this);
            }
        }
    }
}

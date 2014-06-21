using libairvidproto;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace aairvid.Protocol
{
    public class Decoder
    {
        public Encodable Decode(BinaryReader r, string key = null)
        {
            var type = r.ReadChar();
            switch (type)
            {
                case 'o':
                    {
                        var unknow = r.ReadInt32();

                        var namelength = ByteOrderConv.Instance.NetworkToHostOrder(r.ReadInt32());
                        var name = DecodeString(r, namelength);
                        var obj = new RootObj(RootObj.GetType(name));

                        unknow = r.ReadInt32();
                        var childrenCount = ByteOrderConv.Instance.NetworkToHostOrder(r.ReadInt32());
                        for (int i = 0; i < childrenCount; ++i)
                        {
                            var keylen = ByteOrderConv.Instance.NetworkToHostOrder(r.ReadInt32());
                            var key1 = DecodeString(r, keylen);
                            if (key1 == null)
                            {
                                return obj;
                            }
                            var child = Decode(r, key1);
                            obj.Add(child);
                        }
                        return obj;
                    }
                case 's': // string
                    {
                        var id = ByteOrderConv.Instance.NetworkToHostOrder(r.ReadInt32());
                        var payloadLen = ByteOrderConv.Instance.NetworkToHostOrder(r.ReadInt32());
                        return new StringValue(key, DecodeString(r, payloadLen), id);
                    }

                case 'i':
                case 'r': // int
                    {
                        return new IntValue(key, ByteOrderConv.Instance.NetworkToHostOrder(r.ReadInt32()));
                    }
                case 'a':
                case 'e': // array
                    {
                        var unknow = r.ReadInt32();
                        var childrenCount = ByteOrderConv.Instance.NetworkToHostOrder(r.ReadInt32());
                        EncodableList li = new EncodableList();
                        for (int counter = 0; counter < childrenCount; counter++)
                        {
                            li.Add(Decode(r));
                        }
                        return new EncodableValue(key, li);
                    }

                case 'n': // null
                    return new StringValue(key, null);

                case 'f': // 8-byte float
                    {
                        var int64Value = ByteOrderConv.Instance.NetworkToHostOrder(r.ReadInt64());
                        var f = BitConverter.Int64BitsToDouble(int64Value);
                        return new DoubleValue(key, f);
                    }
                case 'x':
                    {
                        var unknow = r.ReadInt32();
                        var payloadLen = ByteOrderConv.Instance.NetworkToHostOrder(r.ReadInt32());
                        return new BytesValue(key, r.ReadBytes(payloadLen));
                    }
                case 'l': // long, int64
                    {
                        var bigInt = ByteOrderConv.Instance.NetworkToHostOrder(r.ReadInt64());
                        return new BigIntValue(key, bigInt);
                    }
                default:
                    {
                        throw new InvalidDataException("unknow data type" + type.ToString());
                    }
            }
        }

        private string DecodeString(BinaryReader r, int payloadLen)
        {
            var chars = Encoding.UTF8.GetChars(r.ReadBytes(payloadLen));
            var str = new string(chars);
            return str;
        }
    }
}

using System.IO;

namespace Network.Dns
{
    public class Answer : IServerResponse
    {
        public DomainName DomainName { get; set; }
        public Type Type { get; set; }
        public Class Class { get; set; }
        public uint Ttl { get; set; }
        public ResponseData ResponseData { get; set; }

        public byte[] GetBytes()
        {
            return BinaryHelper.GetBytes(this);
        }

        public override string ToString()
        {
            return string.Format("{0}, Type: {1}, Class: {2}, TTL: {3} = {4}", DomainName, Type, Class, Ttl, ResponseData);
        }

        internal static Answer Get(BinaryReader reader)
        {
            var a = new Answer
            {
                DomainName = DomainName.Get(reader),
                Type = (Type) BinaryHelper.ReadUInt16(reader),
                Class = (Class) BinaryHelper.ReadUInt16(reader),
                Ttl = BinaryHelper.ReadUInt32(reader)
            };
            a.ResponseData = ResponseData.Get(a.Type, reader);
            return a;
        }

        public void WriteTo(Stream stream)
        {
            DomainName.WriteTo(stream);
            BinaryHelper.Write(stream, (ushort)Type);
            BinaryHelper.Write(stream, (ushort)Class);
            BinaryHelper.Write(stream, Ttl);
            ResponseData?.WriteTo(stream);
        }
    }
}

﻿using System.IO;

namespace Network
{
    public class BinaryRequest : IServerRequest<BinaryRequest>, IClientRequest
    {
        MemoryStream stream = new MemoryStream();

        #region IRequest<BinaryRequest> Members

        public BinaryRequest GetRequest(Stream stream)
        {
            BinaryRequest request = new BinaryRequest();
            byte[] buffer = new byte[1024];
            int length = buffer.Length;
            while (length > 0)
            {
                length = stream.Read(buffer, 0, buffer.Length);
                request.stream.Write(buffer, 0, length);
            }
            return request;
        }

        public void WriteTo(Stream stream)
        {
            this.stream.WriteTo(stream);
        }

        public byte[] GetBytes()
        {
            return stream.ToArray();
        }

        #endregion

        #region IRequest<BinaryRequest> Members


        public BinaryRequest GetRequest(byte[] requestBytes)
        {
            using (MemoryStream stream = new MemoryStream(requestBytes))
            {
                return GetRequest(stream);
            }
        }

        #endregion

        #region IRequest Members

        public void WriteTo(BinaryWriter stream)
        {
            WriteTo(stream.BaseStream);
        }

        #endregion
    }
}

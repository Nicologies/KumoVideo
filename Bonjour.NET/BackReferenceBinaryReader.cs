using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Network.Dns
{
    class BackReferenceBinaryReader : BinaryReader
    {
        public BackReferenceBinaryReader(Stream input)
            : base(input)
        {

        }

        public BackReferenceBinaryReader(Stream input, Encoding encoding)
            : base(input, encoding)
        {

        }

        SortedDictionary<int, object> registeredElements = new SortedDictionary<int, object>();

        public T Get<T>(int p)
        {
            return (T)registeredElements[p];
        }

        public void Register(int p, object value)
        {
            registeredElements.Add(p,value);
        }
    }
}

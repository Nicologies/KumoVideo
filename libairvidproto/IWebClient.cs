using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libairvidproto
{
    public interface IWebClient
    {
        void AddHeader(string key, string value);

        byte[] UploadData(string endPoint, byte[] reqData);
    }
}

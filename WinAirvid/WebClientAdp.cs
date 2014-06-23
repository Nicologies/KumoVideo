using libairvidproto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace WinAirvid
{
    public class WebClientAdp : IWebClient
    {
        private WebClient _client = new WebClient();
        public void AddHeader(string key, string value)
        {
            _client.Headers.Add(key, value);
        }

        public byte[] UploadData(string endPoint, byte[] reqData)
        {
            return _client.UploadData(endPoint, reqData);
        }
    }
}

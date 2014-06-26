using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using libairvidproto;
using System.Net;

namespace aairvid
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
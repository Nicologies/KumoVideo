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
    public class WebClientAdp : WebClient, IWebClient
    {
        void IWebClient.AddHeader(string key, string value)
        {
            Headers.Add(key, value);
        }

        byte[] IWebClient.UploadData(string endPoint, byte[] reqData)
        {
            return (this as WebClient).UploadData(endPoint, reqData);
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);
            request.Timeout = 20 * 1000;
            return request;
        }
    }
}
using aairvid.Protocol;
using aairvid.Utils;
using Android.OS;
using Java.Interop;
using Network.ZeroConf;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace aairvid.Model
{
    public class AVServer : Java.Lang.Object, IParcelable
    {
        private Guid _clientId = Guid.NewGuid();

        public string PasswordDigest = "";

        private IService _service;
        private WebClient _webClient;
        private string _endpoint;
        public string Name
        {
            get;
            private set;
        }

        private enum ServiceType
        {
            Browser,
            PlaybackService,
            PlayWithConvService,
        }

        public enum ActionType
        {
            GetResources,
            GetNestItem,
            InitPlayback,
            InitPlaybackWithConv,
        }

        private static Dictionary<ServiceType, string> ServiceTypeDesc = new Dictionary<ServiceType, string>()
        {
            {ServiceType.Browser,  "browseService"},
            {ServiceType.PlaybackService,  "playbackService"},
            {ServiceType.PlayWithConvService,  "livePlaybackService"},
        };

        private static Dictionary<ActionType, string> ActionTypeDesc = new Dictionary<ActionType, string>()
        {
            {ActionType.GetResources,  "getItems"},
            {ActionType.GetNestItem, "getNestedItem"},
            {ActionType.InitPlayback, "initPlayback"},
            {ActionType.InitPlaybackWithConv, "initLivePlayback"},
        };

        public AVServer(IService service)
        {
            _service = service;
            Name = service.Name;
            var addr = _service.Addresses[0];
            var str = addr.Addresses[0].ToString();
            _endpoint = string.Format("http://{0}:{1}/service", str, addr.Port);

            InitWebClientAndHeaders();
        }

        public AVServer(Parcel source)
        {
            this.Name = source.ReadString();
            this._clientId = new Guid(source.ReadString());
            this.PasswordDigest = source.ReadString();
            this._endpoint = source.ReadString();    
        }

        public void Save(Parcel dest)
        {
            dest.WriteString(Name);
            dest.WriteString(_clientId.ToString());
            dest.WriteString(PasswordDigest);
            dest.WriteString(_endpoint);
        }

        public AVServer()
        {
        }

        private void InitWebClientAndHeaders()
        {
            _webClient = new WebClient();
            var headers = _webClient.Headers;
            headers.Add("User-Agent", "AirVideo/2.4.13 CFNetwork/548.1.4 Darwin/11.0.0");
            headers.Add("Accept", "*/*");
            headers.Add("Accept-Language", "en-us");
            headers.Add("Accept-Encoding", "gzip, deflate");
            headers.Add("Content-Type", "application/x-www-form-urlencoded");
        }

        public List<AVResource> GetResources(string path, ActionType actionType = ActionType.GetResources)
        {
            ServiceType serviceType = ServiceType.Browser;

            var reqData = GetFormData(serviceType, actionType, path);

            var response = _webClient.UploadData(this._endpoint, reqData);

            using (var stream = new MemoryStream(response))
            {
                using (var read = new BinaryReader(stream))
                {
                    var de = new aairvid.Protocol.Decoder();
                    var rootObj = de.Decode(read) as RootObj;
                    var result = rootObj.GetResources(this, actionType);
                    return result;
                }
            }
        }

        private byte[] GetFormData(ServiceType serviceType, ActionType action, string itemId)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream, Encoding.ASCII))
                {
                    var en = new aairvid.Protocol.Encoder(writer);

                    var root = new aairvid.Protocol.RootObj(Protocol.RootObj.EmObjType.ConnectRequest);

                    root.Add(new StringValue("clientIdentifier", this._clientId.ToString()));

                    root.Add(new StringValue("passwordDigest", this.PasswordDigest));

                    root.Add(new StringValue("methodName", ActionTypeDesc[action]));

                    root.Add(new StringValue("requestURL", this._endpoint));

                    var paramList = GetParamList(itemId, action);

                    root.Add(new EncodableValue("parameters", paramList));

                    root.Add(new IntValue("clientVersion", 240));

                    root.Add(new StringValue("serviceName", ServiceTypeDesc[serviceType]));

                    root.Encode(en);

                    return stream.ToArray();
                }
            }
        }

        private static EncodableList GetParamList(string itemId, ActionType action)
        {
            if (action == ActionType.GetResources)
            {
                var paramList = new EncodableList();
                var browserReq = new RootObj(RootObj.EmObjType.BrowserRequest);
                browserReq.Add(new StringValue("folderId", itemId));
                browserReq.Add(new IntValue("preloadDetails", 1));
                browserReq.Add(new IntValue("sortField", 0));
                browserReq.Add(new IntValue("sortDirection", 0));
                paramList.Add(browserReq);
                return paramList;
            }
            else if (action == ActionType.GetNestItem
                || action == ActionType.InitPlayback)
            {
                var paramList = new EncodableList();
                var reqMediaInfo = new StringValue(null, itemId);
                paramList.Add(reqMediaInfo);
                return paramList;
            }
            else if (action == ActionType.InitPlaybackWithConv)
            {
                var paramList = new EncodableList();
                var convReq = new RootObj(RootObj.EmObjType.ConversionRequest);
                convReq.Add(new StringValue("itemId", itemId));
                convReq.Add(new IntValue("audioStream", 1));
                var profile = CodecProfile.GetProfile();
                convReq.Add(new BitratesValue("allowedBitratesLocal", profile.Bitrate.ToString()));
                convReq.Add(new BitratesValue("allowedBitratesRemote", "256"));
                convReq.Add(new DoubleValue("audioBoost", 0));
                convReq.Add(new IntValue("cropRight", 0));
                convReq.Add(new IntValue("cropLeft", 0));
                convReq.Add(new IntValue("resolutionWidth", profile.Width));
                convReq.Add(new IntValue("videoStream", 0));
                convReq.Add(new IntValue("cropBottom", 0));
                convReq.Add(new IntValue("cropTop", 0));
                convReq.Add(new DoubleValue("quality", 0.7));
                convReq.Add(new StringValue("subtitleInfo", null));
                convReq.Add(new DoubleValue("offset", 0.0));
                convReq.Add(new IntValue("resolutionHeight", profile.Height));
                DeviceInfoValue devInfo = new DeviceInfoValue("metaData");
                devInfo.Add(new StringValue(null, "device"));
                devInfo.Add(new StringValue(null, "iPad"));
                devInfo.Add(new StringValue(null, "clientVersion"));
                devInfo.Add(new StringValue(null, "2.4.13"));
                devInfo.Add(new StringValue(null, "h264Passthrough"));
                devInfo.Add(new StringValue(null, "0"));
                convReq.Add(devInfo);
                paramList.Add(convReq);
                return paramList;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public List<AVResource> GetResources()
        {
            return GetResources("");
        }

        internal AVMediaInfo GetMediaInfo(string id)
        {
            var res = GetResources(id, ActionType.GetNestItem);

            return res.Single(r => r is AVMediaInfo) as AVMediaInfo;
        }

        public string GetPlaybackUrl(AVVideo vid)
        {
            ServiceType serviceType = ServiceType.PlaybackService;

            var reqData = GetFormData(serviceType, ActionType.InitPlayback, vid.Id);

            var response = _webClient.UploadData(this._endpoint, reqData);

            using (var stream = new MemoryStream(response))
            {
                using (var read = new BinaryReader(stream))
                {
                    var de = new aairvid.Protocol.Decoder();
                    var rootObj = de.Decode(read) as RootObj;
                    var playbackResp = rootObj.Get(RootObj.EmObjType.PlaybackInitResponse);
                    var url = playbackResp.Get("contentURL") as StringValue;
                    return url.Value;
                }
            }
        }

        internal string GetPlayWithConvUrl(AVVideo vid)
        {
            ServiceType serviceType = ServiceType.PlayWithConvService;

            var reqData = GetFormData(serviceType, ActionType.InitPlaybackWithConv, vid.Id);

            var response = _webClient.UploadData(this._endpoint, reqData);

            using (var stream = new MemoryStream(response))
            {
                using (var read = new BinaryReader(stream))
                {
                    var de = new aairvid.Protocol.Decoder();
                    var rootObj = de.Decode(read) as RootObj;
                    var playbackResp = rootObj.Get(RootObj.EmObjType.PlaybackInitResponse);
                    var url = playbackResp.Get("contentURL") as StringValue;
                    return url.Value;
                }
            }
        }

        #region IParcelable implementation
        public int DescribeContents()
        {
            return 0;
        }

        public void WriteToParcel(Parcel dest, ParcelableWriteFlags flags)
        {
            //dest.WriteString();
        }

        [ExportField("CREATOR")]
        public static AVServerCreator InitializeCreator()
        {
            return new AVServerCreator();
        }
        #endregion
    }

    public class AVServerCreator : Java.Lang.Object, IParcelableCreator
    {
        public Java.Lang.Object CreateFromParcel(Parcel source)
        {
            return new AVServer(source);
        }

        public Java.Lang.Object[] NewArray(int size)
        {
            return new AVServer[size];
        }
    }
}

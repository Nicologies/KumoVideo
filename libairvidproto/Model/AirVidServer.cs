using aairvid.Protocol;
using aairvid.Utils;
using Android.OS;
using Java.Interop;
using Network.ZeroConf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace aairvid.Model
{
    public class AirVidServer : Java.Lang.Object, IParcelable
    {
        public Guid _clientId = Guid.NewGuid();

        public string PasswordDigest = "";

        private IService _service;
        public string _endpoint;
        public string Name
        {
            get;
            private set;
        }

        public enum ServiceType
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

        public static Dictionary<ServiceType, string> ServiceTypeDesc = new Dictionary<ServiceType, string>()
        {
            {ServiceType.Browser,  "browseService"},
            {ServiceType.PlaybackService,  "playbackService"},
            {ServiceType.PlayWithConvService,  "livePlaybackService"},
        };

        public static Dictionary<ActionType, string> ActionTypeDesc = new Dictionary<ActionType, string>()
        {
            {ActionType.GetResources,  "getItems"},
            {ActionType.GetNestItem, "getNestedItem"},
            {ActionType.InitPlayback, "initPlayback"},
            {ActionType.InitPlaybackWithConv, "initLivePlayback"},
        };

        public AirVidServer(IService service)
        {
            _service = service;
            Name = service.Name;
            var addr = _service.Addresses[0];
            var str = addr.Addresses[0].ToString();
            _endpoint = string.Format("http://{0}:{1}/service", str, addr.Port);

            CreateWebClient();
        }

        public AirVidServer(Parcel source)
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

        public AirVidServer()
        {
        }

        private WebClient CreateWebClient()
        {
            var webClient = new WebClient();
            var headers = webClient.Headers;
            headers.Add("User-Agent", "AirVideo/2.4.13 CFNetwork/548.1.4 Darwin/11.0.0");
            headers.Add("Accept", "*/*");
            headers.Add("Accept-Language", "en-us");
            headers.Add("Accept-Encoding", "gzip, deflate");
            headers.Add("Content-Type", "application/x-www-form-urlencoded");
            return webClient;
        }
        
        public List<AirVidResource> GetResources(string path, ActionType actionType = ActionType.GetResources)
        {
            ServiceType serviceType = ServiceType.Browser;

            var reqData = new FormDataGenForGetResources(this, serviceType, actionType, path).GetFormData();

            var webClient = CreateWebClient();

            var response = webClient.UploadData(this._endpoint, reqData);

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

        public List<AirVidResource> GetNestItem(string path)
        {
            ServiceType serviceType = ServiceType.Browser;

            var actionType = ActionType.GetNestItem;

            var reqData = new FormDataGenForNestItem(this, serviceType, actionType, path).GetFormData();

            var webClient = CreateWebClient();

            var response = webClient.UploadData(this._endpoint, reqData);

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

        public List<AirVidResource> GetResources()
        {
            return GetResources("");
        }

        public MediaInfo GetMediaInfo(string id)
        {
            var res = GetNestItem(id);

            return res.Single(r => r is MediaInfo) as MediaInfo;
        }

        public string GetPlaybackUrl(Video vid)
        {
            ServiceType serviceType = ServiceType.PlaybackService;

            var reqData = new FormDataGenForPlayback(this, serviceType, ActionType.InitPlayback, vid.Id).GetFormData();

            var webClient = CreateWebClient();
            var response = webClient.UploadData(this._endpoint, reqData);

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

        public string GetPlayWithConvUrl(Video vid, MediaInfo mediaInfo,
            SubtitleStream sub, ICodecProfile codecProfile)
        {
            ServiceType serviceType = ServiceType.PlayWithConvService;

            var reqData = new FormDataGenForPlaybackWithConv(this,
                serviceType,
                ActionType.InitPlaybackWithConv,
                vid.Id, 
                mediaInfo,
                sub, codecProfile).GetFormData();

            var webClient = CreateWebClient();
            var response = webClient.UploadData(this._endpoint, reqData);

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
            return new AirVidServer(source);
        }

        public Java.Lang.Object[] NewArray(int size)
        {
            return new AirVidServer[size];
        }
    }
}

using aairvid.Utils;
using libairvidproto.types;
using libairvidproto.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace libairvidproto.model
{
    public class AirVidServer
    {
        public Guid _clientId = Guid.NewGuid();

        internal string PasswordDigest = "";

        public void SetPassword(string pwd)
        {
            PasswordDigest = PasswordDigestHelper.GetPasswordHexString("S@17" + pwd + "@1r").ToUpperInvariant();
        }

        private IServer _service;
        public IServer Server
        {
            get
            {
                return _service;
            }
        }
        public string _endpoint;
        public string Name
        {
            get;
            private set;
        }

        public string ID
        {
            get
            {
                return Name + _service.Address + _service.Port.ToString();
            }
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

        public AirVidServer(IServer server)
        {
            _service = server;
            Name = server.Name;

            if (!string.IsNullOrWhiteSpace(server.Password))
            {
                SetPassword(server.Password);
            }

            _endpoint = string.Format("http://{0}:{1}/service", _service.Address, _service.Port);
        }
        
        public AirVidServer()
        {
        }

        private void FillWebClientHeader(IWebClient webClient)
        {
            webClient.AddHeader("User-Agent", "AirVideo/2.4.13 CFNetwork/548.1.4 Darwin/11.0.0");
            webClient.AddHeader("Accept", "*/*");
            webClient.AddHeader("Accept-Language", "en-us");
            webClient.AddHeader("Accept-Encoding", "gzip, deflate");
            webClient.AddHeader("Content-Type", "application/x-www-form-urlencoded");
        }
        
        public List<AirVidResource> GetResources(
            IWebClient webClient,
            string path,
            ActionType actionType = ActionType.GetResources)
        {
            ServiceType serviceType = ServiceType.Browser;

            var reqData = new FormDataGenForGetResources(this, serviceType, actionType, path).GetFormData();

            FillWebClientHeader(webClient);

            var response = webClient.UploadData(this._endpoint, reqData);

            using (var stream = new MemoryStream(response))
            {
                using (var read = new BinaryReader(stream))
                {
                    var de = new Decoder();
                    var rootObj = de.Decode(read) as RootObj;

                    var err = rootObj.Children.SingleOrDefault(r => r is StringValue && (r as StringValue).Key == "errorMessage") as StringValue;
                    if (err != null && err.Value != null && err.Value.ToString().ToUpperInvariant().StartsWith("INVALID PASSWORD"))
                    {
                        throw new InvalidPasswordException();
                    }
                    var result = rootObj.GetResources(this, actionType);
                    return result;
                }
            }
        }

        public List<AirVidResource> GetNestItem(IWebClient webClient, string path)
        {
            ServiceType serviceType = ServiceType.Browser;

            var actionType = ActionType.GetNestItem;

            var reqData = new FormDataGenForNestItem(this, serviceType, actionType, path).GetFormData();

            FillWebClientHeader(webClient);

            var response = webClient.UploadData(this._endpoint, reqData);

            using (var stream = new MemoryStream(response))
            {
                using (var read = new BinaryReader(stream))
                {
                    var de = new Decoder();
                    var rootObj = de.Decode(read) as RootObj;
                    var result = rootObj.GetResources(this, actionType);
                    return result;
                }
            }
        }

        public List<AirVidResource> GetResources(IWebClient webClient)
        {
            return GetResources(webClient, "");
        }

        public MediaInfo GetMediaInfo(IWebClient webClient, string id)
        {
            var res = GetNestItem(webClient, id);

            return res.Single(r => r is MediaInfo) as MediaInfo;
        }

        public string GetPlaybackUrl(IWebClient webClient, Video vid)
        {
            ServiceType serviceType = ServiceType.PlaybackService;

            var reqData = new FormDataGenForPlayback(this, serviceType, ActionType.InitPlayback, vid.Id).GetFormData();

            FillWebClientHeader(webClient);
            var response = webClient.UploadData(this._endpoint, reqData);

            using (var stream = new MemoryStream(response))
            {
                using (var read = new BinaryReader(stream))
                {
                    var de = new Decoder();
                    var rootObj = de.Decode(read) as RootObj;
                    var playbackResp = rootObj.Get(RootObj.EmObjType.PlaybackInitResponse);
                    var url = playbackResp.Get("contentURL") as StringValue;
                    return url.Value;
                }
            }
        }

        public string GetPlayWithConvUrl(IWebClient webClient, Video vid,
            MediaInfo mediaInfo,
            SubtitleStream sub, AudioStream audio, ICodecProfile codecProfile)
        {
            ServiceType serviceType = ServiceType.PlayWithConvService;

            var reqData = new FormDataGenForPlaybackWithConv(this,
                serviceType,
                ActionType.InitPlaybackWithConv,
                vid.Id, 
                mediaInfo,
                sub, audio, codecProfile).GetFormData();

            FillWebClientHeader(webClient);
            var response = webClient.UploadData(this._endpoint, reqData);

            using (var stream = new MemoryStream(response))
            {
                using (var read = new BinaryReader(stream))
                {
                    var de = new Decoder();
                    var rootObj = de.Decode(read) as RootObj;
                    var playbackResp = rootObj.Get(RootObj.EmObjType.PlaybackInitResponse);
                    var url = playbackResp.Get("contentURL") as StringValue;
                    return url.Value;
                }
            }
        }
    }
}

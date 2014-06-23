using aairvid.Utils;
using libairvidproto.types;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace libairvidproto.model
{
    public abstract class FormDataGen : IFormDataGen
    {
        protected AirVidServer _server;
        protected AirVidServer.ServiceType _serviceType;
        protected AirVidServer.ActionType _actType;
        protected string _itemId;
        public FormDataGen(AirVidServer server,
            AirVidServer.ServiceType serviceType,
            AirVidServer.ActionType actType, string itemId)
        {
            _server = server;
            _serviceType = serviceType;
            _actType = actType;
            _itemId = itemId;
        }

        public byte[] GetFormData()
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream, Encoding.UTF8))
                {
                    var en = new libairvidproto.types.Encoder(writer);

                    var root = new RootObj(RootObj.EmObjType.ConnectRequest);

                    root.Add(new StringValue("clientIdentifier", _server._clientId.ToString()));

                    root.Add(new StringValue("passwordDigest", _server.PasswordDigest));

                    root.Add(new StringValue("methodName", AirVidServer.ActionTypeDesc[_actType]));

                    root.Add(new StringValue("requestURL", _server._endpoint));

                    var paramList = GetParamList();

                    root.Add(new EncodableValue("parameters", paramList));

                    root.Add(new IntValue("clientVersion", 240));

                    root.Add(new StringValue("serviceName", AirVidServer.ServiceTypeDesc[_serviceType]));

                    root.Encode(en);

                    return stream.ToArray();
                }
            }
        }
        
        protected abstract EncodableList GetParamList();
    }

    public class FormDataGenForNestItem : FormDataGen
    {
        public FormDataGenForNestItem(AirVidServer server,
            AirVidServer.ServiceType serviceType,
            AirVidServer.ActionType actType, string itemId)
            :base(server, serviceType, actType, itemId)
        {
        }

        protected override EncodableList GetParamList()
        {
            var paramList = new EncodableList();
            var reqMediaInfo = new StringValue(null, _itemId);
            paramList.Add(reqMediaInfo);
            return paramList;
        }
    }

    public class FormDataGenForPlayback : FormDataGenForNestItem
    {
        public FormDataGenForPlayback(AirVidServer server,
            AirVidServer.ServiceType serviceType,
            AirVidServer.ActionType actType, string itemId)
            : base(server, serviceType, actType, itemId)
        {
        }
    }

    public class FormDataGenForPlaybackWithConv : FormDataGen
    {
        SubtitleStream _sub;
        MediaInfo _mediaInfo;
        ICodecProfile _codecProfile;
        public FormDataGenForPlaybackWithConv(AirVidServer server,
            AirVidServer.ServiceType serviceType,
            AirVidServer.ActionType actType,
            string itemId, MediaInfo mediaInfo, SubtitleStream sub, ICodecProfile codecProfile)
            : base(server, serviceType, actType, itemId)
        {
            _sub = sub;
            _mediaInfo = mediaInfo;
            _codecProfile = codecProfile;
        }

        protected override EncodableList GetParamList()
        {
            var paramList = new EncodableList();
            var convReq = new RootObj(RootObj.EmObjType.ConversionRequest);
            convReq.Add(new StringValue("itemId", _itemId));
            convReq.Add(new IntValue("audioStream", 1));
            convReq.Add(new BitratesValue("allowedBitratesLocal", _codecProfile.Bitrate.ToString()));
            convReq.Add(new BitratesValue("allowedBitratesRemote", "256"));
            convReq.Add(new DoubleValue("audioBoost", 0));
            convReq.Add(new IntValue("cropRight", 0));
            convReq.Add(new IntValue("cropLeft", 0));
            convReq.Add(new IntValue("resolutionWidth", _codecProfile.DeviceWidth));
            convReq.Add(new IntValue("videoStream", 0));
            convReq.Add(new IntValue("cropBottom", 0));
            convReq.Add(new IntValue("cropTop", 0));
            convReq.Add(new DoubleValue("quality", 0.7));
            if (_sub == null)
            {
                convReq.Add(new StringValue("subtitleInfo", null));
            }
            else
            {
                convReq.Add(new EncodableValue("subtitleInfo", _sub.SubtitleInfoFromServer));
            }

            var vidStream = _mediaInfo.VideoStreams.First();

            var height = Math.Min(
                (int)((float)_codecProfile.DeviceWidth * (float)vidStream.Height / (float)vidStream.Width),
                _codecProfile.DeviceHeight);

            convReq.Add(new DoubleValue("offset", 0.0));
            convReq.Add(new IntValue("resolutionHeight", height));
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
    }

    public class FormDataGenForGetResources : FormDataGen
    {
        public FormDataGenForGetResources(AirVidServer server,
            AirVidServer.ServiceType serviceType,
            AirVidServer.ActionType actType, string itemId)
            : base(server, serviceType, actType, itemId)
        {
        }

        protected override EncodableList GetParamList()
        {
            var paramList = new EncodableList();
            var browserReq = new RootObj(RootObj.EmObjType.BrowserRequest);
            browserReq.Add(new StringValue("folderId", _itemId));
            browserReq.Add(new IntValue("preloadDetails", 1));
            browserReq.Add(new IntValue("sortField", 0));
            browserReq.Add(new IntValue("sortDirection", 0));
            paramList.Add(browserReq);
            return paramList;
        }
    }
}
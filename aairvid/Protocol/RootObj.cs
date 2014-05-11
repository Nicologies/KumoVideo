using aairvid.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace aairvid.Protocol
{
    public class RootObj : Encodable
    {
        public enum EmObjType
        {
            ConnectRequest,
            BrowserRequest,
            ConnectResponse,
            FolderContent,
            DiskRootFolder,
            Folder,
            ITunesRootFolder,
            VideoItem,
            MediaInfo,
            VideoStream,
            AudioStream,
            SubtitleInfo,
            SubtitleStream,
            PlaybackInitResponse,
            ConversionRequest,
        }

        private static Dictionary<EmObjType, string> OBJ_TYPE_DESC = new Dictionary<EmObjType, string>()
        {
            {EmObjType.ConnectRequest, "air.connect.Request"},
            {EmObjType.BrowserRequest, "air.video.BrowseRequest"},
            {EmObjType.ConnectResponse, "air.connect.Response"},
            {EmObjType.FolderContent, "air.video.FolderContent"},
            {EmObjType.DiskRootFolder, "air.video.DiskRootFolder"},
            {EmObjType.Folder, "air.video.Folder"},
            {EmObjType.ITunesRootFolder, "air.video.ITunesRootFolder"},
            {EmObjType.VideoItem, "air.video.VideoItem"}, 
            {EmObjType.MediaInfo, "air.video.MediaInfo"},    
            {EmObjType.VideoStream, "air.video.MediaInfo.VideoStream"}, 
            {EmObjType.AudioStream, "air.video.MediaInfo.AudioStream"}, 
            {EmObjType.SubtitleInfo, "air.video.SubtitleInfo"},
            {EmObjType.SubtitleStream, "air.video.MediaInfo.SubtitleStream"}, 
            {EmObjType.PlaybackInitResponse, "air.video.PlaybackInitResponse"},
            {EmObjType.ConversionRequest, "air.video.ConversionRequest"}, 
        };

        public RootObj(EmObjType objType) : this(objType, new EncodableList())
        {
        }

        public RootObj(EmObjType objType, Encodable en) : this(objType)
        {
            Children.Add(en);
        }

        public RootObj(EmObjType objType, EncodableList li)
        {
            _objType = objType;
            Children = li;
        }

        public EncodableList Children
        {
            get;
            protected set;
        }
        public virtual void Encode(Encoder encoder)
        {
            encoder.Encode(this);
        }

        public void Add(Encodable en)
        {
            Children.Add(en);
        }

        private EmObjType _objType;

        public string ObjType
        {
            get
            {
                return OBJ_TYPE_DESC[_objType];
            }
        }

        /// <summary>
        /// Don't know what's it.
        /// </summary>
        public int MValue
        {
            get
            {
                if (_objType == EmObjType.ConnectRequest)
                {
                    return 1;
                }
                return 240;
            }
        }

        internal static EmObjType GetType(string name)
        {
            return OBJ_TYPE_DESC.Single(r => r.Value == name).Key;
        }

        public Encodable Get(string key)
        {
            var cs = Children.ToList();
            foreach (var c in cs)
            {
                if (c is RootObj && (c as RootObj).ObjType== key)
                {
                    return c;
                }

                if (c is KeyValueBase && (c as KeyValueBase).Key == key)
                {
                    return c;
                }
            }
            return null;
        }

        internal List<AVResource> GetResources(AVServer server, AVServer.ActionType actionType)
        {
            var res = new List<AVResource>();
            if (actionType == AVServer.ActionType.GetResources)
            {
                var folderContent = Get(OBJ_TYPE_DESC[EmObjType.FolderContent]) as RootObj;
                if (folderContent == null)
                {
                    return res;
                }
                var items = folderContent.Get("items") as EncodableValue;
                if (items == null)
                {
                    return res;
                }
                if (!(items.Value is EncodableList))
                {
                    return res;
                }
                foreach (var item in items.Value as EncodableList)
                {
                    if (!(item is RootObj))
                    {
                        continue;
                    }
                    var itemObj = item as RootObj;
                    var objType = itemObj._objType;
                    if (objType == EmObjType.DiskRootFolder ||
                        objType == EmObjType.Folder ||
                        objType == EmObjType.ITunesRootFolder)
                    {
                        string name;
                        string id;
                        GetNameAndId(itemObj, out name, out id);
                        res.Add(new AVFolder(server, name, id));
                    }
                    else if (objType == EmObjType.VideoItem)
                    {
                        string name;
                        string id;
                        GetNameAndId(itemObj, out name, out id);
                        res.Add(new AVVideo(server, name, id));
                    }
                }
            }
            else if (actionType == AVServer.ActionType.GetNestItem)
            {
                var videoItem = this.Get(EmObjType.VideoItem);
                if (videoItem != null)
                {
                    var avMediaInfo = new AVMediaInfo(server);
                    var mediaInfo = videoItem.Get(EmObjType.MediaInfo);
                    if (mediaInfo != null)
                    {
                        foreach (var c in mediaInfo.Children)
                        {
                            if (c is KeyValueBase)
                            {
                                var keyValue = c as KeyValueBase;
                                if (keyValue.Key == "fileSize")
                                {
                                    var fileSizeObj = keyValue as BigIntValue;
                                    avMediaInfo.FileSize = fileSizeObj.Value;
                                }
                                else if (keyValue.Key == "duration")
                                {
                                    var durationObj = keyValue as DoubleValue;
                                    avMediaInfo.Duration = durationObj.Value;
                                }
                                else if (keyValue.Key == "bitrate")
                                {
                                    var bitrateObj = keyValue as IntValue;
                                    if (bitrateObj == null)
                                    {
                                        avMediaInfo.Bitrate = 0;
                                    }
                                    else
                                    {
                                        avMediaInfo.Bitrate = bitrateObj.Value;
                                    }
                                }
                                else if (keyValue.Key == "videoThumbnail")
                                {
                                    var videoThumbnailObj = keyValue as BytesValue;
                                    avMediaInfo.Thumbnail = videoThumbnailObj.Value;
                                }

                                else if (keyValue.Key == "subtitles")
                                {
                                    var o = keyValue as EncodableValue;
                                    if (o != null)
                                    {
                                        var li = o.Value as EncodableList;
                                    }
                                }

                                else if (keyValue.Key == "streams")
                                {
                                    ParseStreams(avMediaInfo, keyValue);
                                }
                            }
                        }
                    }

                    res.Add(avMediaInfo);
                }
            }
            return res;
        }

        private void ParseStreams(AVMediaInfo avMediaInfo, KeyValueBase keyValue)
        {
            var o = keyValue as EncodableValue;
            if (o != null)
            {
                var li = o.Value as EncodableList;
                foreach (var item in li)
                {
                    var stream = item as RootObj;
                    if (stream == null)
                    {
                        continue;
                    }
                    if (stream._objType == EmObjType.VideoStream)
                    {
                        var vidStream = GetVideoStream(stream);
                        avMediaInfo.VideoStreams.Add(vidStream);
                    }
                    if (stream._objType == EmObjType.AudioStream)
                    {
                        var audioStream = GetAudioStream(stream);
                        avMediaInfo.AudioStreams.Add(audioStream);
                    }
                }
            }
        }

        private AVAudioStream GetAudioStream(RootObj stream)
        {
            var audioStream = new AVAudioStream();
            foreach (var item in stream.Children)
            {
                var keyValue = item as KeyValueBase;
                if (keyValue == null)
                {
                    continue;
                }

                if (keyValue.Key == "index")
                {
                    var intValue = keyValue as IntValue;
                    audioStream.index = intValue.Value;
                }
                else if (keyValue.Key == "streamType")
                {
                    var intValue = keyValue as IntValue;
                    audioStream.StreamType = intValue.Value;
                }
                else if (keyValue.Key == "codec")
                {
                    var strValue = keyValue as StringValue;
                    if (strValue == null)
                    {
                        audioStream.Codec = "";
                    }
                    else
                    {
                        audioStream.Codec = strValue.Value;
                    }
                }
                else if (keyValue.Key == "language")
                {
                    var strValue = keyValue as StringValue;
                    if (strValue == null)
                    {
                        audioStream.Language = "";
                    }
                    else
                    {
                        audioStream.Language = strValue.Value;
                    }
                    
                }
            }
            return audioStream;
        }

        private static AVVideoStream GetVideoStream(RootObj stream)
        {
            var vidStream = new AVVideoStream();
            foreach (var item in stream.Children)
            {
                var keyValue = item as KeyValueBase;
                if (keyValue == null)
                {
                    continue;
                }

                if (keyValue.Key == "index")
                {
                    var intValue = keyValue as IntValue;
                    vidStream.index = intValue.Value;
                }
                else if (keyValue.Key == "width")
                {
                    var intValue = keyValue as IntValue;
                    vidStream.Width = intValue.Value;
                }
                else if (keyValue.Key == "height")
                {
                    var intValue = keyValue as IntValue;
                    vidStream.Height = intValue.Value;
                }
                else if (keyValue.Key == "streamType")
                {
                    var intValue = keyValue as IntValue;
                    vidStream.StreamType = intValue.Value;
                }
                else if (keyValue.Key == "codec")
                {
                    var strValue = keyValue as StringValue;
                    vidStream.Codec = strValue.Value;
                }
                else if (keyValue.Key == "language")
                {
                    var strValue = keyValue as StringValue;
                    vidStream.Language = strValue.Value;
                }
            }
            return vidStream;
        }

        public RootObj Get(EmObjType emObjType)
        {
            return Children.SingleOrDefault(r => r is RootObj && (r as RootObj)._objType == emObjType) as RootObj;
        }

        private static void GetNameAndId(RootObj itemObj, out string name, out string id)
        {
            name = "";
            id = "";
            foreach (var property in itemObj.Children)
            {
                if (property is KeyValueBase)
                {
                    var keyValue = property as KeyValueBase;
                    if (keyValue.Key == "name")
                    {
                        var strValue = (keyValue as StringValue);
                        if (strValue != null)
                        {
                            name = strValue.Value;
                        }
                    }
                    else if (keyValue.Key == "itemId")
                    {
                        var strValue = (keyValue as StringValue);
                        if (strValue != null)
                        {
                            id = strValue.Value;
                        }
                    }
                }
            }
        }
    }
}

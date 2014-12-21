using aairvid.Utils;
using libairvidproto.model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace WinAirvid
{
    public class VideoVM : ResourceVM
    {
        private Video _video;
        private IBusyingSetter _busySetter;
        public VideoVM(Video v, IBusyingSetter busySetter)
        {
            _video = v;
            _busySetter = busySetter;
        }
        public override bool IsSelected
        {
            get
            {
                return _IsSelected;
            }
            set
            {
                if (_IsSelected != value)
                {
                    _IsSelected = value;
                    NotifyPropertyChange(() => IsSelected);
                    LoadMediaInfo();
                }
            }
        }

        private void LoadMediaInfo()
        {
            if (!IsLoaded)
            {
                var t = new Task(DoGetMediaInfoFromServer);
                _busySetter.SetBusying(true);
                t.Start();
            }
        }

        private void DoGetMediaInfoFromServer()
        {
            var mediaInfo = _video.GetMediaInfo(new WebClientAdp());
            RunOnUIThread(() =>
            {
                MediaInfo = mediaInfo;
                _busySetter.SetBusying(false);
                IsLoaded = true;
            });
        }

        public override string Name
        {
            get
            {
                return _video.Name;
            }
        }

        private MediaInfo _MediaInfo;
        public MediaInfo MediaInfo
        {
            get
            {
                return _MediaInfo;
            }
            set
            {
                if (_MediaInfo != value)
                {
                    _MediaInfo = value;
                    NotifyPropertyChange(() => MediaInfo);

                    if (_MediaInfo != null)
                    {
                        Thumbnail = BitmapImageFromByteArray(_MediaInfo.Thumbnail);
                    }
                }
            }
        }

        private static BitmapImage BitmapImageFromByteArray(Byte[] bytes)
        {
            MemoryStream stream = new MemoryStream(bytes);
            stream.Seek(0, SeekOrigin.Begin);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.StreamSource = stream;
            image.EndInit();
            return image;
        }

        private BitmapImage _Thumbnail;
        public BitmapImage Thumbnail
        {
            get
            {
                return _Thumbnail;
            }
            set
            {
                if (_Thumbnail != value)
                {
                    _Thumbnail = value;
                    NotifyPropertyChange(() => Thumbnail);
                }
            }
        }

        private class CodecProfile : ICodecProfile
        {
            public int Bitrate
            {
                get { return 2560; }
            }

            public int DeviceHeight
            {
                get { return (int)System.Windows.SystemParameters.PrimaryScreenHeight; }
            }

            public int DeviceWidth
            {
                get { return (int)System.Windows.SystemParameters.PrimaryScreenWidth; }
            }


            public int Height
            {
                get { return DeviceHeight; }
            }

            public int Width
            {
                get { return DeviceWidth; }
            }
        }

        public string GetPlaybackURL(SubtitleStream subtitle, AudioStream audioStream)
        {
            return _video.GetPlaybackUrl(new WebClientAdp(), _MediaInfo, subtitle, audioStream, new CodecProfile());
        }
    }
}


using aairvid.Utils;
using Android.Content;
using Android.Util;

namespace aairvid.VitamioAdapter
{
    public class VitamioVideoView : IO.Vov.Vitamio.Widget.VideoView
    {
        public VitamioVideoView(Context p0)
            : base(p0)
        {
        }
        public VitamioVideoView(Context p0, IAttributeSet p1)
            : base(p0, p1)
        {
        }

        public VitamioVideoView(Context p0, IAttributeSet p1, int p2)
            : base(p0, p1, p2)
        {
        }

        protected override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();
        }

        public void SetLayoutStretch(float ratio)
        {
            var prof = AndroidCodecProfile.GetProfile();
            SetScreenResolution(prof.DeviceWidth, prof.DeviceHeight);
            SetVideoLayout(IO.Vov.Vitamio.Widget.VideoView.VideoLayoutScale, 0);
        }
    }
}
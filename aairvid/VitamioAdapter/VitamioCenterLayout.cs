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
using Android.Util;

namespace aairvid.VitamioAdapter
{
    public class VitamioCenterLayout : IO.Vov.Vitamio.Widget.CenterLayout
    {
        public VitamioCenterLayout(Context p0)
            : base(p0)
        {
        }
        public VitamioCenterLayout(Context p0, IAttributeSet p1)
            : base(p0, p1)
        {
        }

        public VitamioCenterLayout(Context p0, IAttributeSet p1, int p2)
            : base(p0, p1, p2)
        {
        }
    }
}
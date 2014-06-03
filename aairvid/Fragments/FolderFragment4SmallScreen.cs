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
using aairvid.Adapter;

namespace aairvid.Fragments
{
    public class FolderFragment4SmallScreen : FolderFragment
    {
        public FolderFragment4SmallScreen()
        {
        }
        public FolderFragment4SmallScreen(AirVidResourcesAdapter adp)
            : base(adp)
        {
        }
    }
}
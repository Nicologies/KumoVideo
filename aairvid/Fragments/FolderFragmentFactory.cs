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
using Android.Content.Res;
using aairvid.UIUtils;

namespace aairvid.Fragments
{
    public static class FolderFragmentFactory
    {
        public static FolderFragment GetFolderFragment(AirVidResourcesAdapter adp, Display disp)
        {
            if (ScreenProperty.IsLargeScreen(disp.Width))
            {
                return new FolderFragment4LargeScreen(adp);
            }
            else
            {
                return new FolderFragment4SmallScreen(adp);
            }
        }
    }
}
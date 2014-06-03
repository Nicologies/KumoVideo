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

namespace aairvid
{
    public static class FragmentHelper
    {
        public static void AddFragment(Activity ctx, Fragment fragment, string tag)
        {
            var transaction = ctx.FragmentManager.BeginTransaction();
            transaction.Replace(Resource.Id.fragmentPlaceholder, fragment, tag);
            transaction.AddToBackStack(tag);
            transaction.Commit();
        }
    }
}
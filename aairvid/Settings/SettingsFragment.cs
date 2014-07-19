

using Android.OS;
using Android.Preferences;
using Android.Runtime;
using System;

namespace aairvid
{
	public class SettingsFragment :  PreferenceFragment
	{
		public override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			AddPreferencesFromResource(Resource.Layout.settings);
		}

        public SettingsFragment()
        { }

        protected SettingsFragment(IntPtr javaReference,
            JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }
	}
}


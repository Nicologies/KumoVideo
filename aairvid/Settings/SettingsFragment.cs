﻿

using Android.OS;
using Android.Preferences;

namespace aairvid
{
	public class SettingsFragment :  PreferenceFragment
	{
		public override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);

			AddPreferencesFromResource(Resource.Layout.settings);
		}
	}
}


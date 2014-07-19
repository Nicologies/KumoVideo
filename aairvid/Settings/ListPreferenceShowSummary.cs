using Android.Content;
using Android.Preferences;
using Android.Runtime;
using Android.Util;
using System;

namespace aairvid.Settings
{
    public class ListPreferenceShowSummary : ListPreference
    {
        public ListPreferenceShowSummary(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            Init();
        }

        public ListPreferenceShowSummary(Context context)
            : base(context)
        {
            Init();
        }

        protected ListPreferenceShowSummary(IntPtr javaReference,
            JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
            Init();
        }

        private void Init()
        {
            this.PreferenceChange += ListPreferenceShowSummary_PreferenceChange;
        }

        void ListPreferenceShowSummary_PreferenceChange(object sender, Preference.PreferenceChangeEventArgs e)
        {
            e.Preference.Summary = e.NewValue.ToString();
            this._summary = e.Preference.Summary;
        }

        private string _summary = "";
        public override Java.Lang.ICharSequence SummaryFormatted
        {
            get
            {
                _summary = this.Entry;
                return new Java.Lang.String(_summary);
            }
            set
            {
                _summary = value.ToString();
                base.SummaryFormatted = value;
            }
        }
    }
}

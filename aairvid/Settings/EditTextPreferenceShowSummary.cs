using Android.Content;
using Android.Preferences;
using Android.Runtime;
using Android.Util;
using System;

namespace aairvid.Settings
{
    public class EditTextPreferenceShowSummary : EditTextPreference
    {
        public EditTextPreferenceShowSummary(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            Init();
        }

        public EditTextPreferenceShowSummary(Context context)
            : base(context)
        {
            Init();
        }

        protected EditTextPreferenceShowSummary(IntPtr javaReference,
            JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        private void Init()
        {
            this.PreferenceChange += EditTextPreferenceShowSummary_PreferenceChange;
        }

        void EditTextPreferenceShowSummary_PreferenceChange(object sender, Preference.PreferenceChangeEventArgs e)
        {
            SummaryFormatted = new Java.Lang.String(e.NewValue.ToString());
        }
        private Java.Lang.ICharSequence _summary;
        public override Java.Lang.ICharSequence SummaryFormatted
        {
            get
            {
                if (_summary == null)
                {
                    if (base.Text != null)
                    {
                        _summary = new Java.Lang.String(base.Text);
                    }
                }
                return _summary;
            }
            set
            {
                _summary = value;
                base.SummaryFormatted = value;
            }
        }
    }
}

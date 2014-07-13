using Android.Content;
using Android.Preferences;
using Android.Runtime;
using Android.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace aairvid.Settings
{
    public class ListPreferenceShowSummary : ListPreference
    {
        public ListPreferenceShowSummary(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
        }

        public ListPreferenceShowSummary(Context context)
            : base(context)
        {
        }

        public override Java.Lang.ICharSequence SummaryFormatted
        {
            get
            {
                if (this.Entry != null)
                {
                    return new Java.Lang.String("Current Value: " + this.Entry);
                }
                return new Java.Lang.String("");
            }
            set
            {
                base.SummaryFormatted = value;
            }
        }
    }
}

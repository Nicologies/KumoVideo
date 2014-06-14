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
using Android.Preferences;
using aairvid.Model;

namespace aairvid.Utils
{
    public class RecentLans
    {
        private static readonly string RECENT_LANS = "RecentLans.Languages";
        public static RecentLans Instance = new RecentLans();
        public List<string> _recentLans = new List<string>();
        private static readonly int MAX_LAN = 10;
        public int GetLanWeight(Context ctx, string lan)
        {
            if (string.IsNullOrWhiteSpace(lan) || lan.ToUpperInvariant() == "DISABLED")
            {
                return int.MinValue;
            }

            RetrieveRecentLansFromPref(ctx);

            return _recentLans.IndexOf(lan.ToUpperInvariant());
        }

        private void RetrieveRecentLansFromPref(Context ctx)
        {
            if (_recentLans.Count() == 0)
            {
                var pref = PreferenceManager.GetDefaultSharedPreferences(ctx);
                var lans = pref.GetString(RECENT_LANS, "");
                _recentLans.AddRange(lans.Split(';').Where(r => !string.IsNullOrWhiteSpace(r)));
            }
        }

        public void UpdateRecentLan(Context ctx, string lan)
        {
            lan = lan.ToUpperInvariant();
            if (!_recentLans.Contains(lan))
            {
                if (_recentLans.Count() > MAX_LAN)
                {
                    _recentLans = _recentLans.Take(MAX_LAN).ToList();
                }
                _recentLans.Add(lan);

                var pref = PreferenceManager.GetDefaultSharedPreferences(ctx);
                var editor = pref.Edit();
                editor.PutString(RECENT_LANS, string.Join(";", _recentLans.ToArray()));
                editor.Commit();
            }
            else
            {
                _recentLans.Remove(lan);
                _recentLans.Add(lan);

                var pref = PreferenceManager.GetDefaultSharedPreferences(ctx);
                var editor = pref.Edit();
                editor.PutString(RECENT_LANS, string.Join(";", _recentLans.ToArray()));
                editor.Commit();
            }
        }

        public void UpdateRecentLan(Context ctx, SubtitleStream sub)
        {
            if (sub != null && !string.IsNullOrWhiteSpace(sub.Language.Value))
            {
                UpdateRecentLan(ctx, sub.Language.Value);
            }
        }
    }
}
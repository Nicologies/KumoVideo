using aairvid.Model;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using System.Collections.Generic;
using System.Linq;

namespace aairvid.Adapter
{
    public class SubtitleAdapter : BaseAdapter<SubtitleStream>
    {
        private LayoutInflater _inflater;
        private List<SubtitleStream> _subtitles = new List<SubtitleStream>();
        private Context _context;

        public SubtitleAdapter(Context context)
        {
            _context = context;
            _inflater = LayoutInflater.From(context);
        }

        public override int Count
        {
            get { return _subtitles.Count(); }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override Android.Views.View GetView(int position, Android.Views.View convertView, Android.Views.ViewGroup parent)
        {
            if (convertView == null) // otherwise create a new one
            {
                convertView = new TextView(_context);
            }

            var textView = convertView as TextView;
            
            var item = this[position];
            textView.Text = item.Language;
            return convertView;
        }

        public void Add(SubtitleStream res)
        {
            this._subtitles.Add(res);
            this.NotifyDataSetChanged();
        }

        public override SubtitleStream this[int position]
        {
            get
            {
                if (position >= _subtitles.Count())
                {
                    return null;
                }
                return _subtitles[position];
            }
        }

        public void AddRange(IEnumerable<SubtitleStream> res)
        {
            this._subtitles.AddRange(res);
            this.NotifyDataSetChanged();
        }
    }
}

using aairvid.Model;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using libairvidproto.model;
using System.Collections.Generic;
using System.Linq;

namespace aairvid.Adapter
{
    public class AudioStreamAdapter : BaseAdapter<AudioStreamJavaAdp>
    {
        private LayoutInflater _inflater;
        private List<AudioStreamJavaAdp> audioStreams = new List<AudioStreamJavaAdp>();
        private Context _context;

        public AudioStreamAdapter(Context context)
        {
            _context = context;
            _inflater = LayoutInflater.From(context);
        }

        public override int Count
        {
            get { return audioStreams.Count(); }
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
            if (string.IsNullOrWhiteSpace(item.Stream.Language))
            {
                textView.Text = "Stream " + item.Stream.index.ToString();
            }
            else
            {
                textView.Text = item.Stream.Language;
            }
            return convertView;
        }

        public void Add(AudioStream res)
        {
            this.audioStreams.Add(new AudioStreamJavaAdp(res));
            this.NotifyDataSetChanged();
        }

        public override AudioStreamJavaAdp this[int position]
        {
            get
            {
                if (position >= audioStreams.Count())
                {
                    return null;
                }
                return audioStreams[position];
            }
        }

        public void AddRange(IEnumerable<AudioStream> stream)
        {
            this.audioStreams.AddRange(stream.Select(r => new AudioStreamJavaAdp(r)));
            this.NotifyDataSetChanged();
        }

        internal int GetDefaultAudioStream()
        {
            for(int i = 0; i < audioStreams.Count(); ++i)
            {
                if (audioStreams[i].Stream.index == 1)
                {
                    return i;
                }
            }
            return 0;
        }
    }
}

using aairvid.Model;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using HistoryContainer = System.Collections.Generic.Dictionary<string, aairvid.Model.HistoryItem>;

namespace aairvid.Utils
{
    public static class HistoryMaiten
    {
        private static readonly string HISTORY_FILE_NAME = "./history.bin";
        private static readonly string HISTORY_FILE = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), HISTORY_FILE_NAME);

        private static HistoryContainer _history = new HistoryContainer();

        public static void Load()
        {
            if (_history.Count() == 0)
            {
                if (File.Exists(HISTORY_FILE))
                {
                    using (var stream = File.OpenRead(HISTORY_FILE))
                    {
                        var fmt = new BinaryFormatter();
                        _history = fmt.Deserialize(stream) as HistoryContainer;
                    }
                }
            }

            int maxHis = 200;

            if (_history.Count() > maxHis)
            {
                var temp = _history.OrderBy(r => r.Value.LastPlayDate);
                _history = temp.Skip(temp.Count() / 2).ToDictionary(r => r.Key, r => r.Value);
            }
        }

        public static void SaveLastPos(string vidBaseName, long pos)
        {
            HistoryItem hisItem;
            if (_history.TryGetValue(vidBaseName, out hisItem))
            {
                hisItem.LastPlayDate = DateTime.Now;
                hisItem.LastPosition = pos;
            }
            else
            {
                hisItem = new HistoryItem()
                {
                    LastPosition = pos,
                    LastPlayDate = DateTime.Now
                };
                _history.Add(vidBaseName, hisItem);
            }

            using (var stream = File.OpenWrite(HISTORY_FILE))
            {
                new BinaryFormatter().Serialize(stream, _history);
            }
        }
        public static long GetLastPos(string vidBaseName)
        {
            var item = GetLastPlayedInfo(vidBaseName);
            if (item == null)
            {
                return 0;
            }
            return item.LastPosition;
        }

        public static HistoryItem GetLastPlayedInfo(string vidBaseName)
        {
            HistoryItem hisItem;
            if (_history.TryGetValue(vidBaseName, out hisItem))
            {
                hisItem.LastPlayDate = DateTime.Now;
                return hisItem;
            }
            else
            {
                return null;
            }
        }
    }
}
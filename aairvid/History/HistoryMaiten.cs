using aairvid.Model;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using libairvidproto.model;
using HistoryContainer = System.Collections.Generic.Dictionary<string, aairvid.Model.HistoryItem>;

namespace aairvid.Utils
{
    public static class HistoryMaiten
    {
        private static readonly string HISTORY_FILE_NAME = "./history.bin";
        private static readonly string HISTORY_FILE = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), HISTORY_FILE_NAME);

        public static HistoryContainer HistoryItems = new HistoryContainer();

        public static void Load()
        {
            if (HistoryItems.Count() == 0)
            {
                if (File.Exists(HISTORY_FILE))
                {
                    using (var stream = File.OpenRead(HISTORY_FILE))
                    {
                        var fmt = new BinaryFormatter();
                        HistoryItems = fmt.Deserialize(stream) as HistoryContainer;
                    }
                }
            }

            int maxHis = 200;

            if (HistoryItems.Count() > maxHis)
            {
                var temp = HistoryItems.OrderBy(r => r.Value.LastPlayDate);
                HistoryItems = temp.Skip(temp.Count() / 2).ToDictionary(r => r.Key, r => r.Value);
            }
        }

        public static void SaveLastPos(string vidBaseName, long pos, AirVidResource parent)
        {
            HistoryItem hisItem;
            if (HistoryItems.TryGetValue(vidBaseName, out hisItem))
            {
                hisItem.LastPlayDate = DateTime.Now;
                hisItem.LastPosition = pos;
            }
            else
            {
                hisItem = new HistoryItem()
                {
                    LastPosition = pos,
                    LastPlayDate = DateTime.Now,
                    Server = parent.Server.Name,
                    FolderPath = parent.GetPath(),
                    FolderId = parent.Id,
                };
                HistoryItems.Add(vidBaseName, hisItem);
            }

            using (var stream = File.OpenWrite(HISTORY_FILE))
            {
                new BinaryFormatter().Serialize(stream, HistoryItems);
            }
        }
        public static long GetLastPos(string vidBaseName)
        {
            var item = GetLastPlayedInfo(vidBaseName);
            return item == null ? 0 : item.LastPosition;
        }

        public static HistoryItem GetLastPlayedInfo(string vidBaseName)
        {
            HistoryItem hisItem;
            if (HistoryItems.TryGetValue(vidBaseName, out hisItem))
            {
                hisItem.LastPlayDate = DateTime.Now;
                return hisItem;
            }
            return null;
        }
    }
}
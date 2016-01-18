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
            if (!HistoryItems.Any())
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

            int maxHis = 100;

            if (HistoryItems.Count() > maxHis)
            {
                var temp = HistoryItems.OrderByDescending(r => r.Value.LastPlayDate);
                HistoryItems = temp.Take(temp.Count() / 2).ToDictionary(r => r.Key, r => r.Value);
            }
        }

        public static void SaveLastPos(Video vid, long pos, AirVidResource.NodeInfo parent)
        {
            HistoryItem hisItem;
            if (HistoryItems.TryGetValue(vid.Id, out hisItem))
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
                    Server = vid.Server.Name,
                    ServerId = vid.Server.ID,
                    FolderPath = parent.Path,
                    FolderId = parent.Id,
                    VideoId = vid.Id,
                    VideoName = vid.GetDispName()
                };
                HistoryItems.Add(vid.Id, hisItem);
            }

            SaveAllItems();
        }

        public static void SaveAllItems()
        {
            using (var stream = File.OpenWrite(HISTORY_FILE))
            {
                new BinaryFormatter().Serialize(stream, HistoryItems);
            }
        }

        public static long GetLastPos(string vidId)
        {
            var item = GetLastPlayedInfo(vidId);
            return item == null ? 0 : item.LastPosition;
        }

        public static HistoryItem GetLastPlayedInfo(string vidId)
        {
            HistoryItem hisItem;
            if (HistoryItems.TryGetValue(vidId, out hisItem))
            {
                hisItem.LastPlayDate = DateTime.Now;
                return hisItem;
            }
            return null;
        }
    }
}
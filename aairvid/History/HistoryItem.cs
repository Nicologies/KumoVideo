using System;

namespace aairvid.Model
{
    [Serializable]
    public class HistoryItem
    {
        public long LastPosition { get; set; }
        public DateTime LastPlayDate { get; set; }

        public string Server { get; set; }
        public string FolderPath { get; set; }
        public string FolderId { get; set; }

        public string GetPath(string name)
        {
            return FolderPath + @"/" + name;
        }
    }
}

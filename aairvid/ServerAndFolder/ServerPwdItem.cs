using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace aairvid.ServerAndFolder
{
    [Serializable]
    public class ServerPwdItem
    {
        public string ServerPassword { get; set; }
        public DateTime LastUsedTime { get; set; }
    }
}

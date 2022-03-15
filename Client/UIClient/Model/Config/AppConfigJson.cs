using System.Collections.Generic;
using static UIClient.Model.Core;

namespace UIClient.Model.Config
{
    public class AppConfigJson
    {
        public string HostName { get; set; }
        public ushort Port { get; set; }
        public bool FullScreen { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool Song { get; set; }
    }
}

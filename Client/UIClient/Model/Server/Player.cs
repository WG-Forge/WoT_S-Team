using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace UIClient.Model.Server
{
    public class Player
    {
        public int idx { get; set; }
        public string name { get; set; }
        public bool is_observer { get; set; }
    }
}

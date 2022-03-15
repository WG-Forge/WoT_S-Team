using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIClient.Model.Client
{
    public class LoginCreate
    {
        public string name { get; set; }
        public string password { get; set; }
        public string game { get; set; }
        public int? num_turns { get; set; }
        public int num_players { get; set; }
        public bool is_observer { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UIClient.Model.Server;

namespace UIClient.Model.Client
{
    public class Actions
    {
        public WebAction[] actions { get; set; }
    }

    public class WebAction
    {
        public int player_id { get; set; }
        public Model.WebAction action_type { get; set; }
        public object data { get; set; }
    }

    public class ActionMove
    {
        public int vehicle_id { get; set; }
        public Point3 target { get; set; }
    }

    public class ActionShoot
    {
        public int vehicle_id { get; set; }
        public Point3 target { get; set; }
    }
}

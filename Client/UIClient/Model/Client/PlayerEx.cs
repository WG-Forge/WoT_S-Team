using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using UIClient.Model.Server;

namespace UIClient.Model.Client
{
    public class PlayerEx
    {
        public Player CurrentPlayer { get; set; }
        public Brush TeamColor { get; set; }
    }
}

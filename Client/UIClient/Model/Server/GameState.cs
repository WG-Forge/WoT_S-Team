using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using UIClient.Infrastructure.Controls;

namespace UIClient.Model.Server
{
    public class GameState
    {
        public int num_players { get; set; }
        public int num_turns { get; set; }
        public int current_turn { get; set; }
        public Player[] players { get; set; }
        public Player[] observers { get; set; }
        public int? current_player_idx { get; set; }
        public bool finished { get; set; }
        public Dictionary<int, Vehicle> vehicles { get; set; }
        public Dictionary<int, int[]> attack_matrix { get; set; }
        public int? winner { get; set; }
        public Dictionary<int, WinPoints> win_points { get; set; }
        public Point3[] catapult_usage { get; set; }
    }
}

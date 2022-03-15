using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UIClient.Model.Client.Api
{
    public class AIDll
    {
        const string _PathAIDll = "AI.dll";

        [DllImport(_PathAIDll, CallingConvention = CallingConvention.Cdecl)]
        public extern static IntPtr create_ai();

        [DllImport(_PathAIDll, CallingConvention = CallingConvention.Cdecl)]
        public extern static Result destroy_ai(IntPtr ai);

        [DllImport(_PathAIDll, CallingConvention = CallingConvention.Cdecl)]
        public extern static Result get_action(IntPtr ai,
                                        int curr_player,
                                        IntPtr players, int players_size,
                                        IntPtr vehicle, int vehicle_size,
                                        IntPtr win_points, int win_points_size,
                                        IntPtr attack_matrix, int attack_matrix_size,
                                        IntPtr base_, int base_size,
                                        IntPtr obstacle, int obstacle_size,
                                        IntPtr light_repair, int light_repair_size,
                                        IntPtr hard_repair, int hard_repair_size,
                                        IntPtr catapult, int catapult_size,
                                        IntPtr catapult_usage, int catapult_usage_size,
                                        out action_ret actions);
    }
}

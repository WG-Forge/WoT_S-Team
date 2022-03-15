using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace UIClient.Model.Server
{
    public enum VehicleType : int
    {
        [EnumMember(Value = "medium_tank")]
        СТ,
        [EnumMember(Value = "light_tank")]
        ЛТ,
        [EnumMember(Value = "heavy_tank")]
        ТТ,
        [EnumMember(Value = "at_spg")]
        ПТ,
        [EnumMember(Value = "spg")]
        САУ
    }

    public class Vehicle
    {
        public int player_id { get; set; }
        public VehicleType vehicle_type { get; set; }
        public int health { get; set; }
        public Point3 spawn_position { get; set; }
        public Point3 position { get; set; }
        public int capture_points { get; set; }
        public int shoot_range_bonus { get; set; }
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UIClient.Model.Server
{
    public class Map
    {
        public int size { get; set; }
        public string name { get; set; }
        public SpawnPoints[] spawn_points { get; set; }
        public Content content { get; set; }
    }

    public class Content
    {
        [JsonProperty(PropertyName = "base")]
        public Point3[] _base { get; set; }
        public Point3[] obstacle { get; set; }
        public Point3[] light_repair { get; set; }
        public Point3[] hard_repair { get; set; }
        public Point3[] catapult { get; set; }
    }

    public class SpawnPoints
    {
        public Point3[] medium_tank { get; set; }
        public Point3[] light_tank { get; set; }
        public Point3[] heavy_tank { get; set; }
        public Point3[] at_spg { get; set; }
        public Point3[] spg { get; set; }
    }
}

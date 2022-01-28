﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace UIClient.Model
{
    public class PlayerEx
    {
        public Player player;
        public Brush color;
    }

    public class Player
    {
        public int idx { get; set; }
        public string name { get; set; }
        public string password { get; set; }
        public bool is_observer { get; set; }
    }
}

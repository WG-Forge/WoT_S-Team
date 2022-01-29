﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using UIClient.Model;
using UIClient.ViewModel;

namespace UIClient.Infrastructure.Converters
{
    public class ConvertWinPointsToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Dictionary<int, WinPoints> win_points) return value;
            if (parameter is not ViewModelLocator loc) return value;
            var players = loc?.WGUIPageViewModel?.Core?.Field?.players;
            if (players == null) return value;

            StringBuilder hex = new StringBuilder();

            foreach (var i in win_points)
            {
                Model.PlayerEx player;
                if (!players.TryGetValue(i.Key, out player))
                    continue;
                hex.Append(player.player.name).Append(": очки захвата: ").Append(i.Value.capture);
                hex.Append(", убийства: ").Append(i.Value.kill).Append("\n");
            }
            return hex.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
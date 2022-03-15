using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using UIClient.Model.Client;
using UIClient.ViewModel;

namespace UIClient.Infrastructure.Converters
{
    public class ConvertAttackMatrixToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Dictionary<int, int[]> attack_matrix) return value;
            if (parameter is not ViewModelLocator loc) return value;
            var players = loc?.GamePageViewModel?.Field?.Players;
            if (players == null) return value;

            StringBuilder hex = new StringBuilder();

            foreach (var item in attack_matrix)
            {
                PlayerEx player;
                if (!players.TryGetValue(item.Key, out player))
                    continue;
                hex.Append(player.CurrentPlayer.name).Append(": ");
                foreach (var i in item.Value)
                {
                    if (!players.TryGetValue(i, out player))
                        continue;
                    hex.Append(player.CurrentPlayer.name).Append(" ");
                }
                hex.Append("\n");
            }
            return hex.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}

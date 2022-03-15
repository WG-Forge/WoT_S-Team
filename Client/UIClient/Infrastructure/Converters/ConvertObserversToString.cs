using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using UIClient.Model;
using UIClient.Model.Server;
using UIClient.ViewModel;

namespace UIClient.Infrastructure.Converters
{
    public class ConvertObserversToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Player[] observers) return value;
            if(observers == null) return value;
            if (parameter is not ViewModelLocator loc) return value;

            StringBuilder hex = new StringBuilder();

            foreach (var i in observers)
            {
                hex.Append(i.name).Append("\n");
            }
            return hex.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}

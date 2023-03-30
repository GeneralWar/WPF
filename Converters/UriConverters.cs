using System;
using System.Globalization;
using System.Windows.Data;

namespace General.WPF
{
    public class UriUnescapeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Uri.UnescapeDataString(((Uri)value).ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new Uri(Uri.EscapeDataString((string)value));
        }
    }
}

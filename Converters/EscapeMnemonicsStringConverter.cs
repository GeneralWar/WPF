using System;
using System.Globalization;
using System.Windows.Data;

namespace General.WPF
{
    public class EscapeMnemonicsStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string content = value as string ?? "";
            return content != null ? content.Replace("_", "__") : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string content = value as string ?? "";
            return content != null ? content.Replace("__", "_") : value;
        }
    }
}

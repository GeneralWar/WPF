using System;
using System.Globalization;
using System.Windows.Data;

namespace General.WPF
{
    public class DecimalToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((decimal)value).ToString(parameter as string);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            decimal number;
            decimal.TryParse(value as string, out number);
            return number;
        }
    }
}

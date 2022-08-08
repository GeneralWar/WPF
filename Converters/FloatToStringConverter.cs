using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace General.WPF
{
    public class FloatToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((float)value).ToString(parameter as string);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Visibility)
            {
                return false;
            }
            return Visibility.Visible == (Visibility)value ? true : false;
        }
    }
}

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace General.WPF
{
    public class IntegerToVisibilityHiddenConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not int)
            {
                return Visibility.Hidden;
            }

            if (parameter is int)
            {
                return (int)value == (int)parameter ? Visibility.Visible : Visibility.Hidden;
            }

            return 0 == (int)value ? Visibility.Hidden : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

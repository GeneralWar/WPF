using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace General.WPF
{
    public class BooleanToVisibilityCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool visible = value is bool ? (bool)value : false;
            return visible ? Visibility.Visible : Visibility.Collapsed;
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

    public class BooleanToVisibilityHiddenConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool visible = value is bool ? (bool)value : false;
            return visible ? Visibility.Visible : Visibility.Hidden;
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

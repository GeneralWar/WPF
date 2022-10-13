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
            object final = value ?? parameter;
            bool visible = final is bool ? (bool)final : false;
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

    public class InverseBooleanToVisibilityCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object final = value ?? parameter;
            bool invisible = final is bool ? (bool)final : true;
            return invisible ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BooleanToVisibilityHiddenConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object final = value ?? parameter;
            bool visible = final is bool ? (bool)final : false;
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

    public class InverseBooleanToVisibilityHiddenConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            object final = value ?? parameter;
            bool invisible = final is bool ? (bool)final : true;
            return invisible ? Visibility.Hidden : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

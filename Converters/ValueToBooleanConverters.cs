using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace General.WPF
{
    public class EqualityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (0 == values.Length)
            {
                return false;
            }
            if (1 == values.Length)
            {
                return true;
            }
            object first = values[0];
            return values.Skip(1).All(v => v.Equals(first));
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(value as IConvertible)?.ToBoolean(null) ?? false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return this.Convert(value, targetType, parameter, culture);
        }
    }

    public class IntegerToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is int && 0 != (int)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

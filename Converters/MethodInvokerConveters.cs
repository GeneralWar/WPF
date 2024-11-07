using System;
using System.Globalization;
using System.Windows.Data;

namespace General.WPF
{
    public class MethodInvokerConveter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is string method)
            {
                return value.CallMethod(method);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

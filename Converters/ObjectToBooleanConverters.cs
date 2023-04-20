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
}

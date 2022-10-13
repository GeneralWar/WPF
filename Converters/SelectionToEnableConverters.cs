using System;
using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace General.WPF
{
    public class IListSelectionToEnableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            IList? list = value as IList;
            return list is not null && 0 != list.Count;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

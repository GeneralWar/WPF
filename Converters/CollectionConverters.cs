using System;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace General.WPF
{
    /// <summary>
    /// Convert centimeter string like "1cm" to pixels in double type
    /// </summary>
    public class CollectionEmptyToVisibilityCollapsedConverter : IValueConverter
    {
        static private readonly LengthConverter LengthConverter = new LengthConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is IEnumerable collection && collection.GetEnumerator().MoveNext() ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

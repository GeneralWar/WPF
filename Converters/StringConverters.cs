using System;
using System.Globalization;
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
            float number;
            float.TryParse(value as string, out number);
            return number;
        }
    }

    public class DoubleToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((double)value).ToString(parameter as string);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double number;
            double.TryParse(value as string, out number);
            return number;
        }
    }

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

    public class ObjectToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString() ?? value.GetType().ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EscapeMnemonicsStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string content = value as string ?? "";
            return content != null ? content.EscapeToWindowsPresentationFoundationHeader() : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string content = value as string ?? "";
            return content != null ? content.UnescapeFromWindowsPresentationFoundationHeader() : value;
        }
    }

    public class UriStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value as Uri ?? throw new InvalidCastException()).LocalPath;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new Uri(value as string ?? throw new InvalidCastException());
        }
    }

    public class DateTimeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

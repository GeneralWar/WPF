using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace General.WPF
{
    /// <summary>
    /// Convert centimeter string like "1cm" to pixels in double type
    /// </summary>
    public class CentimeterToPixelConverter : IValueConverter
    {
        static private readonly LengthConverter LengthConverter = new LengthConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string? content = (value as string)?.Trim();
            if (string.IsNullOrWhiteSpace(content) || !content.EndsWith("cm"))
            {
                return 0d;
            }

            /*Graphics graphics = Graphics.FromHwnd(Process.GetCurrentProcess().MainWindowHandle);
            if (graphics is null)
            {
                return 0d;
            }

            double result;
            double.TryParse(content.Substring(0, content.Length - 2), out result);
            return result * graphics.DpiX / 2.54d;*/
            return (double)(LengthConverter.ConvertFromString(content) ?? 0d);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

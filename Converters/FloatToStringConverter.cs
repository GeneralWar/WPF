﻿using System;
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
}

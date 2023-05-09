using System;
using System.Globalization;
using System.Windows.Data;

namespace General.WPF
{
    internal class DefaultPercentInputValueTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not double)
            {
                return "0";
            }

            double d = (double)value;
            return (((value as IConvertible)?.ToDouble(null) ?? .0) * 100).ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double d;
            double.TryParse(value as string ?? "", out d);
            return d * 0.01;
        }
    }

    /// <summary>
    /// NumberInputBox.xaml 的交互逻辑
    /// </summary>
    public partial class PercentInputBox : NumberInputBox
    {
        static PercentInputBox()
        {
            NumberInputBox.ValueProperty.AddOwner(typeof(PercentInputBox));
        }

        public PercentInputBox()
        {
            InitializeComponent();
        }

        protected override IValueConverter getValueTextConverter()
        {
            return new DefaultPercentInputValueTextConverter();
        }

        protected override bool TryParse(string text, out double value)
        {
            if (!base.TryParse(text, out value))
            {
                return false;
            }

            value = value * .01;
            return true;
        }

        protected override string checkString(double value)
        {
            return base.checkString(value * 100.0);
        }
    }
}

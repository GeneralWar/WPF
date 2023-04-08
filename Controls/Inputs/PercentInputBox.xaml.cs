using System.Windows;

namespace General.WPF
{
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

        protected override string checkString(double value)
        {
            return base.checkString(value * 100.0);
        }

        protected override double checkValue(double valueFromText)
        {
            return valueFromText * .01;
        }
    }
}

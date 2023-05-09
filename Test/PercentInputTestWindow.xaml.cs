using System.Diagnostics;
using System.Windows;

namespace Genera.WPF.Test
{
    /// <summary>
    /// PercentInputTestWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PercentInputTestWindow : Window
    {
        static private readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(double), typeof(NumberInputTestWindow), new PropertyMetadata(1.0));
        public double Value { get => (double)this.GetValue(ValueProperty); set => this.SetValue(ValueProperty, value); }

        public PercentInputTestWindow()
        {
            InitializeComponent();
        }


        private void onValueChanging(General.WPF.NumberInputBox input, double value)
        {
            Trace.WriteLine($"{nameof(onValueChanging)}: {value}");
        }

        private void onValueChanged(General.WPF.NumberInputBox input, double value)
        {
            Trace.WriteLine($"{nameof(onValueChanged)}: {value}");
            this.Value = value;
        }
    }
}

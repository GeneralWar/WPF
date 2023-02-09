using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Genera.WPF.Test
{
    /// <summary>
    /// NumberInputTestWindow.xaml 的交互逻辑
    /// </summary>
    public partial class NumberInputTestWindow : Window
    {
        public NumberInputTestWindow()
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
        }
    }
}

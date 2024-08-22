using System.Windows;
using System.Windows.Controls;

namespace General.WPF
{
    /// <summary>
    /// HeaderedTextBox.xaml 的交互逻辑
    /// </summary>
    public partial class HeaderedTextBox : TextBox
    {
        static private readonly DependencyProperty HeaderWidthProperty = DependencyProperty.Register(nameof(HeaderWidth), typeof(double), typeof(HeaderedTextBox));
        public double HeaderWidth { get => (double)this.GetValue(HeaderWidthProperty); set => this.SetValue(HeaderWidthProperty, value); }

        static private readonly DependencyProperty HeaderProperty = DependencyProperty.Register(nameof(Header), typeof(string), typeof(HeaderedTextBox));
        public string? Header { get => this.GetValue(HeaderProperty) as string; set => this.SetValue(HeaderProperty, value); }

        static private readonly DependencyProperty SpaceProperty = DependencyProperty.Register(nameof(Space), typeof(GridLength), typeof(HeaderedTextBox));
        public GridLength Space { get => (GridLength)this.GetValue(SpaceProperty); set => this.SetValue(SpaceProperty, value); }

        public HeaderedTextBox()
        {
            InitializeComponent();
        }
    }
}

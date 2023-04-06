using System.Windows;
using System.Windows.Controls;

namespace General.WPF
{
    /// <summary>
    /// HeaderLabel.xaml 的交互逻辑
    /// </summary>
    public partial class HeaderLabel : Label
    {
        static private readonly DependencyProperty HeaderProperty = DependencyProperty.Register(nameof(Header), typeof(string), typeof(HeaderLabel), new PropertyMetadata("Header"));
        public string Header { get => (string)this.GetValue(HeaderProperty); set => this.SetValue(HeaderProperty, value); }

        static private readonly DependencyProperty HeaderWidthProperty = DependencyProperty.Register(nameof(HeaderWidth), typeof(double), typeof(HeaderLabel), new PropertyMetadata(32.0));
        public double HeaderWidth { get => (double)this.GetValue(HeaderWidthProperty); set => this.SetValue(HeaderWidthProperty, value); }

        static private readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(HeaderLabel), new PropertyMetadata("Text"));
        public string Text { get => (string)this.GetValue(TextProperty); set => this.SetValue(TextProperty, value); }

        public HeaderLabel()
        {
            InitializeComponent();
        }
    }
}

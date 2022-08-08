using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace General.WPF
{
    /// <summary>
    /// Hyperlink.xaml 的交互逻辑
    /// </summary>
    public partial class Hyperlink : Label
    {
        static private DependencyProperty PROPERTY_TEXT = DependencyProperty.Register(nameof(Hyperlink.Text), typeof(string), typeof(Hyperlink));
        public string Text { get => this.GetValue(PROPERTY_TEXT) as string ?? ""; set => this.SetValue(PROPERTY_TEXT, value); }

        public event RoutedEventHandler? Click = null;

        public Hyperlink()
        {
            InitializeComponent();
        }

        private void onHyperlinkClick(object sender, RoutedEventArgs e)
        {
            this.Click?.Invoke(this, e);
        }
    }
}

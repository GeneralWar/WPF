using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace General.WPF
{
    /// <summary>
    /// Hyperlink.xaml 的交互逻辑
    /// </summary>
    public partial class LinkLabel : Label
    {
        static public readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(LinkLabel.Text), typeof(string), typeof(LinkLabel));
        public string Text { get => this.GetValue(TextProperty) as string ?? ""; set => this.SetValue(TextProperty, value); }

        public event RoutedEventHandler? Click = null;

        public LinkLabel()
        {
            InitializeComponent();
        }

        private void onHyperlinkClick(object sender, RoutedEventArgs e)
        {
            this.Click?.Invoke(this, e);
        }
    }
}

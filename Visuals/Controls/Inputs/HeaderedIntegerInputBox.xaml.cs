using System.Windows;
using System.Windows.Controls;

namespace General.WPF
{
    /// <summary>
    /// HeaderedTextBox.xaml 的交互逻辑
    /// </summary>
    public partial class HeaderedIntegerInputBox : IntegerInputBox
    {
        static private readonly DependencyProperty HeaderWidthProperty = DependencyProperty.Register(nameof(HeaderWidth), typeof(GridLength), typeof(HeaderedIntegerInputBox));
        public GridLength HeaderWidth { get => (GridLength)this.GetValue(HeaderWidthProperty); set => this.SetValue(HeaderWidthProperty, value); }

        static private readonly DependencyProperty HeaderProperty = DependencyProperty.Register(nameof(Header), typeof(string), typeof(HeaderedIntegerInputBox));
        public string? Header { get => this.GetValue(HeaderProperty) as string; set => this.SetValue(HeaderProperty, value); }

        static private readonly DependencyProperty SpaceProperty = DependencyProperty.Register(nameof(Space), typeof(GridLength), typeof(HeaderedIntegerInputBox));
        public GridLength Space { get => (GridLength)this.GetValue(SpaceProperty); set => this.SetValue(SpaceProperty, value); }

        public HeaderedIntegerInputBox()
        {
            InitializeComponent();
        }
    }
}

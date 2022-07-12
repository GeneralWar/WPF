using System.Windows;

namespace General.WPF
{
    /// <summary>
    /// DataGridHyperLinkColumn.xaml 的交互逻辑
    /// </summary>
    public partial class DataGridHyperlinkColumn : System.Windows.Controls.DataGridHyperlinkColumn
    {
        public event RoutedEventHandler? Click = null;

        public DataGridHyperlinkColumn()
        {
            InitializeComponent();
        }

        private void onDataGridHyperlinkClick(object sender, RoutedEventArgs e)
        {
            this.Click?.Invoke(sender, e);
        }
    }
}

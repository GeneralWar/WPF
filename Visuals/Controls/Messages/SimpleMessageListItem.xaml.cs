using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace General.WPF
{
    /// <summary>
    /// SimpleMessageListItem.xaml 的交互逻辑
    /// </summary>
    public partial class SimpleMessageListItem : MessageListItem
    {
        static private readonly DependencyProperty MessageProperty = DependencyProperty.Register(nameof(SimpleMessageListItem.Message), typeof(string), typeof(SimpleMessageListItem));

        public string Message { get => (string)this.GetValue(MessageProperty); set => this.SetValue(MessageProperty, value); }

        public SimpleMessageListItem() : this("") { }

        public SimpleMessageListItem(string message)
        {
            InitializeComponent();

            this.Message = message;
        }
    }
}

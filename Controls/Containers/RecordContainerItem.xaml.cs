using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace General.WPF
{
    public interface IRecordContainerItem
    {
        object? Item { get; }
    }

    /// <summary>
    /// RecordContainerItem.xaml 的交互逻辑
    /// </summary>
    public partial class RecordContainerItem : UserControl, IRecordContainerItem
    {
        static private readonly DependencyProperty ItemProperty = DependencyProperty.Register(nameof(Item), typeof(object), typeof(RecordContainerItem));
        public object? Item { get => this.GetValue(ItemProperty); set => this.SetValue(ItemProperty, value); }

        private Action<RecordContainerItem> mOnDelete;

        internal RecordContainerItem(Action<RecordContainerItem> onDelete)
        {
            mOnDelete = onDelete;
            InitializeComponent();
        }

        private void onDeleteButtonClick(object sender, RoutedEventArgs e)
        {
            mOnDelete.Invoke(this);
        }
    }
}

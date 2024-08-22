using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace General.WPF
{
    public class RecordContainerAddButtonEvent : EventArgs
    {
        //public List<object> AddItems { get; } = new List<object>();
    }

    public delegate void RecordContainerAddButtonEventDelegate(object sender, RecordContainerAddButtonEvent e);
    public delegate void RecordContainerRecordRemoveEventDelegate(object sender, IRecordContainerItem record);

    /// <summary>
    /// RecordContainer.xaml 的交互逻辑
    /// </summary>
    public partial class RecordContainer : UserControl
    {
        static private readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(nameof(ItemsSource), typeof(ObservableCollection<object>), typeof(RecordContainer));
        public ObservableCollection<object> ItemsSource { get => (ObservableCollection<object>)this.GetValue(ItemsSourceProperty); set => this.SetValue(ItemsSourceProperty, value); }

        static private readonly DependencyProperty AddButtonPositionProperty = DependencyProperty.Register(nameof(AddButtonPosition), typeof(HorizontalAlignment), typeof(RecordContainer));
        public HorizontalAlignment AddButtonPosition { get => (HorizontalAlignment)this.GetValue(AddButtonPositionProperty); set => this.SetValue(AddButtonPositionProperty, value); }

        public event RecordContainerAddButtonEventDelegate? AddButtonClick;

        public event RecordContainerRecordRemoveEventDelegate? RemoveButtonClick;

        public delegate void RecordContainerItemMouseDoubleClick(RecordContainer container, object record);
        public event RecordContainerItemMouseDoubleClick? ItemMouseDoubleClick;

        public RecordContainer()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (ItemsSourceProperty == e.Property)
            {
                ObservableCollection<object>? oldValue = e.OldValue as ObservableCollection<object>;
                if (oldValue is not null)
                {
                    oldValue.CollectionChanged -= this.onItemsCollectionChanged;
                }

                ObservableCollection<object>? newValue = e.NewValue as ObservableCollection<object>;
                if (newValue is not null)
                {
                    foreach (object item in newValue)
                    {
                        RecordContainerItem record = new RecordContainerItem(this.onRecordDeleteButtonClick) { Item = item };
                        record.MouseDoubleClick += this.onItemMouseDoubleClick;
                        mRecordListView.Items.Add(record);
                    }

                    newValue.CollectionChanged += this.onItemsCollectionChanged;
                }
            }
        }

        private void onItemsCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    if (e.NewItems is not null)
                    {
                        int index = e.NewStartingIndex;
                        foreach (object item in e.NewItems)
                        {
                            mRecordListView.Items.Insert(index++, new RecordContainerItem(this.onRecordDeleteButtonClick) { Item = item });
                        }
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    foreach (RecordContainerItem item in mRecordListView.Items.OfType<RecordContainerItem>())
                    {
                        item.MouseDoubleClick -= this.onItemMouseDoubleClick;
                    }
                    mRecordListView.Items.Clear();
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    if (e.OldItems is not null)
                    {
                        int count = e.OldItems.Count;
                        int index = e.OldStartingIndex;
                        while (count-- > 0)
                        {
                            if (mRecordListView.Items[index] is RecordContainerItem item)
                            {
                                item.MouseDoubleClick -= this.onItemMouseDoubleClick;
                            }
                            mRecordListView.Items.RemoveAt(index);
                        }
                    }
                    break;
                default: throw new NotImplementedException($"{nameof(RecordContainer)}.{nameof(ItemsSource)}: not implemented for action {e.Action}");
            }
        }

        private void onAddButtonClick(object sender, RoutedEventArgs _)
        {
            RecordContainerAddButtonEvent e = new RecordContainerAddButtonEvent();
            this.AddButtonClick?.Invoke(this, e);

            /*if (0 == e.AddItems.Count)
            {
                return;
            }

            foreach (object item in e.AddItems)
            {
                mRecordListView.Items.Add(new RecordContainerItem(this.onRecordDeleteButtonClick) { Item = item });
            }*/
        }

        private void onRecordDeleteButtonClick(RecordContainerItem item)
        {
            //mRecordListView.Items.Remove(item);
            this.RemoveButtonClick?.Invoke(this, item);
        }

        private void onItemMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is RecordContainerItem record && record.Item is not null)
            {
                this.ItemMouseDoubleClick?.Invoke(this, record.Item);
            }
        }
    }
}

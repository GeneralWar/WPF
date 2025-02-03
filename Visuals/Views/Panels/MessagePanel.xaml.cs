using General.WPF.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace General.WPF
{
    public class MessagePanelRecord
    {
        public DateTime Time { get; }

        public string Message { get; }
        public SolidColorBrush Color { get; }

        public MessagePanelRecord(string message, SolidColorBrush color) : this(DateTime.Now, message, color) { }

        public MessagePanelRecord(DateTime time, string message, SolidColorBrush color)
        {
            this.Time = time;
            this.Message = message;
            this.Color = color;
        }
    }

    /// <summary>
    /// MessagePanel.xaml 的交互逻辑
    /// </summary>
    public partial class MessagePanel : UserControl
    {
        static private readonly DependencyProperty MaxRecordCountProperty = DependencyProperty.Register(nameof(MaxRecordCount), typeof(int), typeof(MessagePanel), new PropertyMetadata(/*Debugger.IsAttached ? 10 : */300));
        public int MaxRecordCount { get => (int)this.GetValue(MaxRecordCountProperty); set => this.SetValue(MaxRecordCountProperty, value); }

        static private readonly DependencyProperty MessageRecordsProperty = DependencyProperty.Register(nameof(MessageRecords), typeof(ObservableCollection<MessagePanelRecord>), typeof(MessagePanel), new PropertyMetadata(new ObservableCollection<MessagePanelRecord>()));
        public ObservableCollection<MessagePanelRecord> MessageRecords { get => (ObservableCollection<MessagePanelRecord>)this.GetValue(MessageRecordsProperty); set => this.SetValue(MessageRecordsProperty, value); }

        static private readonly DependencyProperty MessageItemsSourceProperty = DependencyProperty.Register(nameof(MessageItemsSource), typeof(ObservableCollection<TextBox>), typeof(MessagePanel), new PropertyMetadata(new ObservableCollection<TextBox>()));
        internal ObservableCollection<TextBox> MessageItemsSource { get => (ObservableCollection<TextBox>)this.GetValue(MessageItemsSourceProperty); }

        public AutoScrollToBottom? AutoScrollToBottom { get; private set; }

        public CancelEventHandler? OnClearItems;
        private bool mIsClearingItems = false;

        private Queue<TextBox> mCachedTextBoxes = new Queue<TextBox>();
        private List<TextBox> mUsedTextBoxes = new List<TextBox>();

        public MessagePanel()
        {
            InitializeComponent();

            this.ContextMenu = new ContextMenu();
            this.initialize(this.ContextMenu);

            this.MessageRecords.CollectionChanged += this.onMessageRecordsCollectionChanged;
        }

        private void initialize(ContextMenu menu)
        {
            MenuItem item = new MenuItem
            {
                Header = "清屏",
            };
            item.Click += this.onClearItemsClick;
            menu.Items.Add(item);
        }

        private TextBox createItem(MessagePanelRecord record)
        {
            lock (this)
            {
                TextBox item;
                if (mCachedTextBoxes.Count > 0)
                {
                    item = mCachedTextBoxes.Dequeue();
                }
                else
                {
                    if (mUsedTextBoxes.Count > this.MaxRecordCount)
                    {
                        item = this.MessageItemsSource.ElementAt(0);
                        this.MessageItemsSource.RemoveAt(0);
                    }
                    else
                    {
                        item = new TextBox();
                        item.TextWrapping = System.Windows.TextWrapping.Wrap;
                        item.Background = new SolidColorBrush(Colors.Transparent);
                        item.BorderThickness = new System.Windows.Thickness();
                        item.IsReadOnly = true;
                    }
                }

                item.Foreground = record.Color;
                item.Text = $"[{record.Time.TimeOfDay.ToGeneralString(true)}] {record.Message}";
                mUsedTextBoxes.Add(item);
                return item;
            }
        }

        private void cacheItem(TextBox item)
        {
            lock (this)
            {
                mUsedTextBoxes.Remove(item);
                mCachedTextBoxes.Enqueue(item);
            }
        }

        private void clearItems()
        {
            lock (this)
            {
                mUsedTextBoxes.ForEach(mCachedTextBoxes.Enqueue);
                mUsedTextBoxes.Clear();
            }
        }

        /*private void scrollToBottom()
        {
            mScrollView.ScrollToVerticalOffset(mStackPanel.ActualHeight);
        }*/

        private void insertMessageRecords(int index, IEnumerable<MessagePanelRecord> records)
        {
            this.Dispatcher.InvokeAsync(() =>
            {
                int i = index;
                foreach (MessagePanelRecord record in records)
                {
                    TextBox? item = this.MessageItemsSource.FirstOrDefault(i => i.Tag == record);
                    if (item is not null)
                    {
                        continue;
                    }

                    int finalIndex = Math.Min(this.MessageItemsSource.Count, index);
                    if (finalIndex < this.MessageItemsSource.Count)
                    {
                        this.MessageItemsSource.Insert(finalIndex, this.createItem(record));
                    }
                    else
                    {
                        this.MessageItemsSource.Add(this.createItem(record));
                    }
                }
            });
        }

        private void removeMessageRecords(int index, IEnumerable<MessagePanelRecord> records)
        {
            this.Dispatcher.InvokeAsync(() =>
            {
                foreach (MessagePanelRecord record in records)
                {
                    TextBox? item = this.MessageItemsSource.FirstOrDefault(i => i.Tag == record);
                    if (item is null)
                    {
                        continue;
                    }

                    this.MessageItemsSource.Remove(item);
                    this.cacheItem(item);
                }
            });
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (MessageRecordsProperty == e.Property)
            {
                if (e.OldValue is ObservableCollection<MessagePanelRecord> oldValue)
                {
                    oldValue.CollectionChanged -= this.onMessageRecordsCollectionChanged;
                }
                if (e.NewValue is ObservableCollection<MessagePanelRecord> newValue)
                {
                    newValue.CollectionChanged += this.onMessageRecordsCollectionChanged;
                }
            }
        }

        private void onRecordPanelLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement target)
            {
                this.AutoScrollToBottom ??= new AutoScrollToBottom(target);
            }
        }

        private void onMessageRecordsCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (mIsClearingItems)
            {
                return;
            }

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Replace:
                    if ((NotifyCollectionChangedAction.Remove == e.Action || NotifyCollectionChangedAction.Replace == e.Action) && e.OldItems is not null)
                    {
                        this.removeMessageRecords(e.OldStartingIndex, e.OldItems.OfType<MessagePanelRecord>());
                    }
                    if ((NotifyCollectionChangedAction.Add == e.Action || NotifyCollectionChangedAction.Replace == e.Action) && e.NewItems is not null)
                    {
                        this.insertMessageRecords(e.NewStartingIndex, e.NewItems.OfType<MessagePanelRecord>());
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                case NotifyCollectionChangedAction.Move:
                    Tracer.Log($"{nameof(MessagePanel)}: unhandled collection change action {e.Action}");
                    break;
            }
        }

        private void onClearItemsClick(object sender, System.Windows.RoutedEventArgs _)
        {
            CancelEventArgs e = new CancelEventArgs();
            this.OnClearItems?.Invoke(this, e);
            if (e.Cancel)
            {
                return;
            }

            try
            {
                mIsClearingItems = true;
                this.MessageRecords.Clear();
                this.MessageItemsSource.Clear();
                this.clearItems();
            }
            finally
            {
                mIsClearingItems = false;
            }
        }
    }
}

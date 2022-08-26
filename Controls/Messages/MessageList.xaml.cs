using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace General.WPF
{
    /// <summary>
    /// MessageList.xaml 的交互逻辑
    /// </summary>
    public partial class MessageList : UserControl
    {
        static private readonly DependencyProperty HorizontalScrollBarVisibilityProperty = DependencyProperty.Register(nameof(HorizontalScrollBarVisibility), typeof(ScrollBarVisibility), typeof(MessageList));
        static private readonly DependencyProperty VerticalScrollBarVisibilityProperty = DependencyProperty.Register(nameof(VerticalScrollBarVisibility), typeof(ScrollBarVisibility), typeof(MessageList));

        static private readonly DependencyProperty MaxMessageCountProperty = DependencyProperty.Register(nameof(MaxMessageCount), typeof(int), typeof(MessageList));

        private List<MessageListItem> mItems = new List<MessageListItem>();
        /// <summary>
        /// Create and return a new array for all messages, do not call it frequently.
        /// </summary>
        public MessageListItem[] Items => mItems.ToArray();

        public ScrollBarVisibility HorizontalScrollBarVisibility { get => (ScrollBarVisibility)this.GetValue(HorizontalScrollBarVisibilityProperty); set => this.SetValue(HorizontalScrollBarVisibilityProperty, value); }
        public ScrollBarVisibility VerticalScrollBarVisibility { get => (ScrollBarVisibility)this.GetValue(VerticalScrollBarVisibilityProperty); set => this.SetValue(VerticalScrollBarVisibilityProperty, value); }

        public int MaxMessageCount { get => (int)this.GetValue(MaxMessageCountProperty); set => this.SetValue(MaxMessageCountProperty, value); }

        private bool mVerticalToEnd = true;
        private double mVerticalOffset = 0;

        public MessageList()
        {
            InitializeComponent();
        }

        public void AppendItem(MessageListItem item)
        {
            if (mItems.Count > this.MaxMessageCount)
            {
                this.RemoveItem(mItems[0]);
            }

            mItems.Add(item);
            mStackPanel.Children.Add(item);
            item.Unloaded += this.onMessageItemUnload;
        }

        private void onMessageItemUnload(object sender, RoutedEventArgs e)
        {
            MessageListItem? item = sender as MessageListItem;
            if (item is null)
            {
                return;
            }

            MessageList? list = item.FindAncestor<MessageList>();
            if (list is null || this != list)
            {
                this.RemoveItem(item);
            }
        }

        public void RemoveItem(MessageListItem item)
        {
            item.Unloaded -= this.onMessageItemUnload;
            mStackPanel.Children.Remove(item);
            mItems.Remove(item);
        }

        private void onScrollViewerScroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
        {
            mVerticalOffset = mScrollViewer.VerticalOffset;
            double distanceToVerticalEnd = mStackPanel.ActualHeight - mScrollViewer.ActualHeight - mScrollViewer.VerticalOffset;
            mVerticalToEnd = Math.Abs(distanceToVerticalEnd) <= 3.0; // allowable error
        }

        private void onStackPanelSizeChanged(object sender, SizeChangedEventArgs e)
        {
            mScrollViewer.ScrollToVerticalOffset(mVerticalToEnd ? mStackPanel.ActualHeight : mVerticalOffset);
        }
    }
}

using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace General.WPF.Helpers
{
    public class AutoScrollToBottom
    {
        private bool mScrollToBottom = true;

        public AutoScrollToBottom(FrameworkElement target)
        {
            target.AddHandler(ScrollBar.ScrollEvent, new ScrollEventHandler(this.onTaskPanelScrollBarScroll));
            target.AddHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(this.onTaskPanelScrollChanged));
            target.AddHandler(FrameworkElement.PreviewMouseWheelEvent, new MouseWheelEventHandler(this.onPreviewMouseWheel));
            target.Unloaded += this.onTargetUnloaded;
        }

        private void onPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is ScrollViewer scroll)
            {
                //mScrollToBottom = e.Delta < 0 && scroll.ExtentHeight - scroll.VerticalOffset <= scroll.ViewportHeight;
                //Tracer.Log("onPreviewMouseWheel");
            }
        }

        private void onTaskPanelScrollBarScroll(object sender, System.Windows.Controls.Primitives.ScrollEventArgs e)
        {
            if (sender is ScrollViewer scroll)
            {
                if (ScrollEventType.ThumbTrack == e.ScrollEventType)
                {
                    mScrollToBottom = false;
                    //Tracer.Log($"onTaskPanelScrollBarScroll: {e.ScrollEventType}");
                }
                else if (ScrollEventType.EndScroll == e.ScrollEventType)
                {
                    //mScrollToBottom = scroll.ExtentHeight - scroll.VerticalOffset <= scroll.ViewportHeight;
                    mScrollToBottom = true;
                    //Tracer.Log($"onTaskPanelScrollBarScroll: {e.ScrollEventType}");
                }
            }
        }

        private void onTaskPanelScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.OriginalSource != e.Source)
            {
                return;
            }

            if (0 != e.ViewportHeightChange || 0 != e.ExtentHeightChange)
            {
                //bool scrollToBottom = e.ExtentHeight - e.VerticalOffset <= e.ViewportHeight;
                //Tracer.Log($"scrollToBottom: {scrollToBottom}, {e.ExtentHeight} - {e.VerticalOffset}, {e.ViewportHeight}");
                if (mScrollToBottom/* && !scrollToBottom*/)
                {
                    (e.OriginalSource as ScrollViewer)?.ScrollToBottom();
                }
            }
        }

        private void onTargetUnloaded(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement target)
            {
                target.RemoveHandler(ScrollBar.ScrollEvent, new ScrollEventHandler(this.onTaskPanelScrollBarScroll));
                target.RemoveHandler(ScrollViewer.ScrollChangedEvent, new ScrollChangedEventHandler(this.onTaskPanelScrollChanged));
                target.RemoveHandler(FrameworkElement.PreviewMouseWheelEvent, new MouseWheelEventHandler(this.onPreviewMouseWheel));
                target.Unloaded -= this.onTargetUnloaded;
            }
        }
    }
}

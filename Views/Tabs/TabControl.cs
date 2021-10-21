using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace General.WPF
{
    class TabControl : System.Windows.Controls.TabControl
    {
        public event Action<TabControl, NotifyCollectionChangedEventArgs>? onItemsChanged = null;

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            this.onItemsChanged?.Invoke(this, e);
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);

            ContentPresenter? content = this.Template.FindName("PART_SelectedContentHost", this) as ContentPresenter;
            if (content is not null && !content.Margin.Equals(new Thickness(0)))
            {
                content.Margin = new Thickness(0);
            }
        }
    }
}

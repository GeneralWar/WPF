using System;
using System.Collections.Specialized;

namespace General.WPF
{
    class TabControl : System.Windows.Controls.TabControl
    {
        public event Action<TabControl, NotifyCollectionChangedEventArgs> onItemsChanged = null;

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            this.onItemsChanged?.Invoke(this, e);
        }
    }
}

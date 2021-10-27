using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace General.WPF
{
    /// <summary>
    /// TreeView.xaml 的交互逻辑
    /// </summary>
    public partial class TreeView : System.Windows.Controls.TreeView, ITreeViewItemCollection, IMultipleSelectionsCollection
    {
        static private readonly PropertyInfo? IsSelectionChangeActiveProperty = typeof(TreeView).GetProperty("IsSelectionChangeActive", BindingFlags.NonPublic | BindingFlags.Instance);
        static private readonly DependencyPropertyKey SelectedItemPropertyKey;

        static TreeView()
        {
            DependencyPropertyKey? key = typeof(System.Windows.Controls.TreeView).GetField("SelectedItemPropertyKey", BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(null) as DependencyPropertyKey;
            if (key is null)
            {
                throw new NullReferenceException();
            }
            SelectedItemPropertyKey = key;
        }

        public delegate void OnSelectedItemsChange(IEnumerable<TreeViewItem> items);
        public event OnSelectedItemsChange? onSelectedItemsChange = null;

        public Brush InactiveSelectionBackground { get; set; } = new SolidColorBrush(Color.FromArgb(128, 144, 144, 144));

        private HashSet<IMultipleSelectionsItem> mSelectedItems = new HashSet<IMultipleSelectionsItem>();
        IEnumerable<IMultipleSelectionsItem> IMultipleSelectionsCollection.SelectedItems => mSelectedItems;
        public IEnumerable<TreeViewItem> SelectedItems => mSelectedItems.Where(i => i is TreeViewItem).Cast<TreeViewItem>().ToArray();

        private IMultipleSelectionsItem? mLastOperatedItem = null;

        public TreeView()
        {
            InitializeComponent();
            IsSelectionChangeActiveProperty?.SetValue(this, true);
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            this.onItemsChange(this, e);
        }

        private void onItemsChange(object sender, NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            if (NotifyCollectionChangedAction.Remove == e.Action)
            {
                if (null == e.OldItems)
                {
                    return;
                }

                int selectedCount = mSelectedItems.Count;
                foreach (object i in e.OldItems)
                {
                    this.removeItem(i);
                }
                if (mSelectedItems.Count != selectedCount)
                {
                    this.reportSelectedItemsChange();
                }
            }
            else if (NotifyCollectionChangedAction.Add == e.Action)
            {
                if (null == e.NewItems)
                {
                    return;
                }

                int selectedCount = mSelectedItems.Count;
                foreach (object i in e.NewItems)
                {
                    this.addItem(i);
                }
                if (mSelectedItems.Count != selectedCount)
                {
                    this.reportSelectedItemsChange();
                }
            }
        }

        private void removeItem(object item)
        {
            TreeViewItem? treeItem = item as TreeViewItem;
            if (treeItem is not null)
            {
                treeItem.onItemsChange -= this.onItemsChange;
                if (treeItem == mLastOperatedItem)
                {
                    mLastOperatedItem = null;
                }
            }

            IMultipleSelectionsItem? selectable = item as IMultipleSelectionsItem;
            if (selectable is not null)
            {
                mSelectedItems.Remove(selectable);
            }
        }

        private void addItem(object item)
        {
            TreeViewItem? treeItem = item as TreeViewItem;
            if (treeItem is not null)
            {
                treeItem.onItemsChange += this.onItemsChange;
            }

            IMultipleSelectionsItem? selectable = item as IMultipleSelectionsItem;
            if (selectable is not null && selectable.IsSelected)
            {
                mSelectedItems.Add(selectable);
            }
        }

        private TreeViewItem[] getItemPath(TreeViewItem? item)
        {
            if (item is null)
            {
                return new TreeViewItem[0];
            }

            TreeViewItem? current = item;
            List<TreeViewItem> path = new List<TreeViewItem>();
            do
            {
                path.Insert(0, current);
                current = current.Parent as TreeViewItem;
            } while (null != current);
            return path.ToArray();
        }

        private void selectItems(IMultipleSelectionsItem from, IMultipleSelectionsItem to)
        {
            if (from == to)
            {
                return;
            }

            TreeViewItem[] fromPath = this.getItemPath(from as TreeViewItem);
            TreeViewItem[] toPath = this.getItemPath(to as TreeViewItem);

            int forkDepth = 0;
            ItemCollection forkCollection = this.Items;
            int shallow = Math.Min(fromPath.Length, toPath.Length);
            for (int i = 0; i < shallow; ++i, ++forkDepth)
            {
                if (fromPath[i] != toPath[i])
                {
                    break;
                }
                forkCollection = fromPath[i].Items;
            }

            int fromIndex = forkCollection.IndexOf(fromPath[forkDepth]);
            int toIndex = forkCollection.IndexOf(toPath[forkDepth]);
            int minIndex = Math.Min(fromIndex, toIndex);
            int maxIndex = Math.Max(fromIndex, toIndex);
            List<IMultipleSelectionsItem> items = new List<IMultipleSelectionsItem>();
            for (int i = minIndex; i <= maxIndex; ++i)
            {
                TreeViewItem? item = forkCollection[i] as TreeViewItem;
                if (item is null)
                {
                    continue;
                }
                this.enumerateItems(item, items);
            }

            fromIndex = items.IndexOf(from);
            toIndex = items.IndexOf(to);
            minIndex = Math.Min(fromIndex, toIndex);
            maxIndex = Math.Max(fromIndex, toIndex);

            if (maxIndex > minIndex)
            {
                foreach (IMultipleSelectionsItem item in items.Skip(minIndex).Take(maxIndex - minIndex + 1))
                {
                    (this as IMultipleSelectionsCollection).Append(item);
                }
                this.reportSelectedItemsChange();
            }
            mLastOperatedItem = to;
        }

        private void enumerateItems(TreeViewItem root, List<IMultipleSelectionsItem> items)
        {
            items.Add(root);
            if (root.IsExpanded)
            {
                foreach (object i in root.Items)
                {
                    TreeViewItem? item = i as TreeViewItem;
                    if (item is null)
                    {
                        continue;
                    }

                    this.enumerateItems(item, items);
                }
            }
        }

        private void reportSelectedItemsChange()
        {
            this.onSelectedItemsChange?.Invoke(this.SelectedItems);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            this.checkClearSelectedItems(e);
        }

        private void checkClearSelectedItems(MouseButtonEventArgs e)
        {
            IInputElement? input = this.InputHitTest(e.GetPosition(this));
            if (input.FindAncestor<TreeViewItem>() is not null)
            {
                return;
            }

            this.ClearAllSelections();
        }

        private void setAsBaseSelected(IMultipleSelectionsItem? item)
        {
            if (this.SelectedItem == item)
            {
                return;
            }

            object selected = this.SelectedItem;
            var selectedItem = selected as System.Windows.Controls.TreeViewItem;
            if (selectedItem is not null)
            {
                selectedItem.IsSelected = false;
            }

            SetValue(SelectedItemPropertyKey, item);

#pragma warning disable CS8604 // 引用类型参数可能为 null。
            RoutedPropertyChangedEventArgs<object> e = new RoutedPropertyChangedEventArgs<object>(selected, item, SelectedItemChangedEvent);
#pragma warning restore CS8604 // 引用类型参数可能为 null。
            OnSelectedItemChanged(e);

            Trace.Assert(1 != mSelectedItems.Count || this.SelectedItem == mSelectedItems.First());
        }

        void IMultipleSelectionsCollection.Select(IMultipleSelectionsItem item)
        {
            foreach (IMultipleSelectionsItem record in mSelectedItems)
            {
                if (record == item)
                {
                    continue;
                }
                record.IsSelected = false;
            }
            mSelectedItems.Clear();
            (this as IMultipleSelectionsCollection).Append(item);
            this.setAsBaseSelected(item);
            this.reportSelectedItemsChange();
        }

        void IMultipleSelectionsCollection.SelectTo(IMultipleSelectionsItem item)
        {
            if (mLastOperatedItem == item)
            {
                return;
            }

            if (mLastOperatedItem is not null)
            {
                this.selectItems(mLastOperatedItem, item);
            }
        }

        void IMultipleSelectionsCollection.Append(IMultipleSelectionsItem item)
        {
            mSelectedItems.Add(item);
            item.IsSelected = true;
            (item as FrameworkElement)?.Focus();
            mLastOperatedItem = item;
        }

        void IMultipleSelectionsCollection.Unselect(IMultipleSelectionsItem item)
        {
            mSelectedItems.Remove(item);
            item.IsSelected = false;
            if (this.SelectedItem == item)
            {
                this.setAsBaseSelected(mSelectedItems.FirstOrDefault());
            }

            mLastOperatedItem = item;
        }

        public void ClearAllSelections()
        {
            foreach (IMultipleSelectionsItem item in mSelectedItems)
            {
                item.IsSelected = false;
            }
            mSelectedItems.Clear();

            mLastOperatedItem = null;
            this.reportSelectedItemsChange();
        }

        void IMultipleSelectionsCollection.ClearAllSelections()
        {
            this.ClearAllSelections();
        }
    }
}

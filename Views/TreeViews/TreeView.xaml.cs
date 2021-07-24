using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
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
    public partial class TreeView : System.Windows.Controls.TreeView
    {
        static private readonly PropertyInfo IsSelectionChangeActiveProperty = typeof(TreeView).GetProperty("IsSelectionChangeActive", BindingFlags.NonPublic | BindingFlags.Instance);

        public delegate void OnSelectedItemsChange(IEnumerable<TreeViewItem> items);
        public event OnSelectedItemsChange onSelectedItemsChange = null;

        public Brush InactiveSelectionBackground { get; set; } = new SolidColorBrush(Color.FromArgb(128, 144, 144, 144));

        private HashSet<TreeViewItem> mSelectedItems = new HashSet<TreeViewItem>();
        public IEnumerable<TreeViewItem> SelectedItems => mSelectedItems;

        private TreeViewItem mLastOperatedItem = null;

        public TreeView()
        {
            InitializeComponent();
            IsSelectionChangeActiveProperty.SetValue(this, true);
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
                    TreeViewItem item = i as TreeViewItem;
                    if (null == item)
                    {
                        continue;
                    }

                    item.MouseDown -= this.onItemMouseDown;
                    item.PreviewMouseDown -= this.onItemPreviewMouseDown;
                    mSelectedItems.Remove(item);
                    if (item == mLastOperatedItem)
                    {
                        mLastOperatedItem = null;
                    }
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

                foreach (object i in e.NewItems)
                {
                    TreeViewItem item = i as TreeViewItem;
                    if (null == item)
                    {
                        continue;
                    }

                    item.MouseDown += this.onItemMouseDown;
                    item.PreviewMouseDown += this.onItemPreviewMouseDown;
                    item.onItemsChange += this.onItemsChange;
                }
            }
        }

        private void onItemMouseDown(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine(sender);
        }

        private TreeViewItem getTreeViewItem(FrameworkElement element)
        {
            FrameworkElement current = element;
            while (null != current && current is not TreeViewItem)
            {
                current = current.TemplatedParent as FrameworkElement;
            }
            return current as TreeViewItem;
        }

        private void onItemPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (MouseButton.Left != e.ChangedButton && MouseButton.Right != e.ChangedButton)
            {
                return;
            }

            TreeViewItem item = sender as TreeViewItem;
            if (null == item)
            {
                return;
            }

            IInputElement hit = item.InputHitTest(e.GetPosition(item));
            item = this.getTreeViewItem(hit as FrameworkElement);
            if (null == item || !item.IsHeaderArea(hit))
            {
                return;
            }

            e.Handled = true;

            if (this.handleItemClick(item, e))
            {
                //this.reportSelectedItemsChange();
            }
        }

        private bool handleItemClick(TreeViewItem item, MouseButtonEventArgs e)
        {
            if (null == item)
            {
                return false;
            }

            bool isControlPressed = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            bool isShiftPressed = !isControlPressed && Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            if (isControlPressed)
            {
                if (item.IsSelected)
                {
                    this.unselect(item);
                    return true;
                }
                else
                {
                    this.append(item);
                    return true;
                }
            }
            else if (isShiftPressed)
            {
                if (mLastOperatedItem == item)
                {
                    return false;
                }

                this.selectItems(mLastOperatedItem, item);
            }
            else
            {
                if (MouseButton.Right == e.ChangedButton && mSelectedItems.Contains(item))
                {
                    return false;
                }
                if (item.IsSelected && 1 == mSelectedItems.Count && mSelectedItems.Contains(item))
                {
                    return false;
                }

                this.select(item);
                return true;
            }

            return false;
        }

        private TreeViewItem[] getItemPath(TreeViewItem item)
        {
            if (null == item)
            {
                return new TreeViewItem[0];
            }

            TreeViewItem current = item;
            List<TreeViewItem> path = new List<TreeViewItem>();
            do
            {
                path.Insert(0, current);
                current = current.Parent as TreeViewItem;
            } while (null != current);
            return path.ToArray();
        }

        private void selectItems(TreeViewItem from, TreeViewItem to)
        {
            if (from == to)
            {
                return;
            }

            TreeViewItem[] fromPath = this.getItemPath(from);
            TreeViewItem[] toPath = this.getItemPath(to);

            int forkDepth = 0;
            ItemCollection forkCollection = this.Items;
            int shallow = Math.Min(fromPath.Length, toPath.Length);
            for (int i = 0; i < shallow; ++i, ++forkDepth)
            {
                if (fromPath[i] == toPath[i])
                {
                    forkCollection = fromPath[i].Items;
                }
                else
                {
                    break;
                }
            }

            int fromIndex = forkCollection.IndexOf(fromPath[forkDepth]);
            int toIndex = forkCollection.IndexOf(toPath[forkDepth]);
            int minIndex = Math.Min(fromIndex, toIndex);
            int maxIndex = Math.Max(fromIndex, toIndex);
            List<TreeViewItem> items = new List<TreeViewItem>();
            for (int i = minIndex; i <= maxIndex; ++i)
            {
                TreeViewItem item = forkCollection[i] as TreeViewItem;
                if (null == item)
                {
                    continue;
                }
                this.enumerateItems(item, items);
            }

            fromIndex = items.IndexOf(from);
            toIndex = items.IndexOf(to);
            minIndex = Math.Min(fromIndex, toIndex);
            maxIndex = Math.Max(fromIndex, toIndex);

            this.append(items.Skip(minIndex).Take(maxIndex - minIndex + 1).ToArray(), to);
            mLastOperatedItem = to;
        }

        private void enumerateItems(TreeViewItem root, List<TreeViewItem> items)
        {
            items.Add(root);
            if (root.IsExpanded)
            {
                foreach (object i in root.Items)
                {
                    TreeViewItem item = i as TreeViewItem;
                    if (null == item)
                    {
                        continue;
                    }

                    this.enumerateItems(item, items);
                }
            }
        }

        private void select(TreeViewItem item)
        {
            this.clearSelectedItems();
            item.IsSelected = true;
            mSelectedItems.Add(mLastOperatedItem = item);
            item.Focus();
            this.reportSelectedItemsChange();
        }

        private void append(TreeViewItem item)
        {
            item.IsSelected = true;
            mSelectedItems.Add(mLastOperatedItem = item);
            item.Focus();
            this.reportSelectedItemsChange();
        }

        private void append(TreeViewItem[] items, TreeViewItem last = null)
        {
            if (null == items || 0 == items.Length)
            {
                return;
            }

            foreach (TreeViewItem item in items)
            {
                item.IsSelected = true;
                mSelectedItems.Add(mLastOperatedItem = item);
            }

            if (null != last)
            {
                mLastOperatedItem = last;
            }

            mLastOperatedItem.Focus();
            this.reportSelectedItemsChange();
        }

        private void unselect(TreeViewItem item)
        {
            item.IsSelected = false;
            mLastOperatedItem = item;
            mSelectedItems.Remove(item);
            this.reportSelectedItemsChange();
        }

        private void reportSelectedItemsChange()
        {
            this.onSelectedItemsChange?.Invoke(mSelectedItems);
        }

        private void clearSelectedItems()
        {
            Array.ForEach(mSelectedItems.ToArray(), item => item.IsSelected = false);
            mSelectedItems.Clear();
            mLastOperatedItem = null;
        }

        public void ClearSelectedItems()
        {
            this.clearSelectedItems();
            this.reportSelectedItemsChange();
        }
    }
}

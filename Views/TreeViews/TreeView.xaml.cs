using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
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
        public delegate void OnSelectedItemsChange(IEnumerable<TreeViewItem> items);
        public event OnSelectedItemsChange? onSelectedItemsChange = null;

        public Brush InactiveSelectionBackground { get; set; } = new SolidColorBrush(Color.FromArgb(128, 144, 144, 144));

        private HashSet<IMultipleSelectionsItem> mSelectedItems = new HashSet<IMultipleSelectionsItem>();
        IEnumerable<IMultipleSelectionsItem> IMultipleSelectionsCollection.SelectedItems => mSelectedItems;
        public IEnumerable<TreeViewItem> SelectedItems => mSelectedItems.Where(i => i is TreeViewItem).Cast<TreeViewItem>().ToArray();
        private IEnumerable<TreeViewItem> mReportedSelectedItems = new TreeViewItem[0];

        private IMultipleSelectionsItem? mLastOperatedItem = null;

        public bool AllowItemDrag { get { return (bool)GetValue(AllowItemDragProperty); } set { SetValue(AllowItemDragProperty, value); } }
        public DragModes ItemDragMode { get { return (DragModes)GetValue(ItemDragModeProperty); } set { SetValue(ItemDragModeProperty, value); } }
        /// <summary>
        /// The valid range to check insert
        /// </summary>
        public double InsertEffectRange { get { return (double)GetValue(InsertEffectRangeProperty); } set { SetValue(InsertEffectRangeProperty, value); } }

        public delegate void OnDragEventDelegate(DragEvent e);
        public event OnDragEventDelegate? onDragOver = null;
        public event OnDragEventDelegate? onDrop = null;

        private bool mDragCancel = false;
        private Border? mDragEffectMask = null;

        public TreeView()
        {
            InitializeComponent();
            IsSelectionChangeActiveProperty?.SetValue(this, true);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            mDragEffectMask = this.Template.FindName("DragEffectMask", this) as Border;
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            //base.OnItemsChanged(e);
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

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            if (this.checkClearSelectedItems(e))
            {
                this.Focus();
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (MouseButtonState.Pressed == e.LeftButton)
            {
                if (!mDragCancel && this.AllowItemDrag)
                {
                    TreeViewItem? item = this.InputHitTest(e.GetPosition(this))?.FindAncestor<TreeViewItem>();
                    if (item?.AllowDrop ?? false)
                    {
                        item.AllowDrop = false;
                        DragDropEffects result = DragDrop.DoDragDrop(item, item, DragDropEffects.All);
                        mDragCancel = DragDropEffects.None == result;
                        item.AllowDrop = true;

                        this.hideDragMask();
                    }
                }
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            mDragCancel = false;
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            base.OnDragOver(e);
            this.showDragMask(e);
        }

        protected override void OnDrop(DragEventArgs e)
        {
            base.OnDrop(e);

            TreeViewItem? sourceItem, targetItem;
            DragModes mode = this.checkDragMode(e, out sourceItem, out targetItem);
            if (DragModes.None == mode || targetItem is null || sourceItem is null)
            {
                e.Effects = DragDropEffects.None;
                this.hideDragMask();
                return;
            }

            DragEvent drag = new DragEvent(sourceItem, targetItem, mode);
            this.onDrop?.Invoke(drag);
            if (drag.Canceled)
            {
                e.Effects = DragDropEffects.None;
                this.hideDragMask();
                return;
            }

            this.hideDragMask();
        }

        protected override void OnDragLeave(DragEventArgs e)
        {
            base.OnDragLeave(e);

            if (this == this.InputHitTest(e.GetPosition(this))?.FindAncestor<TreeView>())
            {
                return;
            }

            this.hideDragMask();
        }

        private DragModes checkDragMode(DragEventArgs e, out TreeViewItem? sourceItem, out TreeViewItem? targetItem)
        {
            targetItem = this.InputHitTest(e.GetPosition(this))?.FindAncestor<TreeViewItem>();
            if (targetItem is null)
            {
                sourceItem = null;
                return DragModes.None;
            }

            sourceItem = e.Data.GetData(typeof(TreeViewItem)) as TreeViewItem;
            if (sourceItem is not null)
            {
                if (sourceItem == targetItem && sourceItem.Parent == targetItem || sourceItem.IsAncestorOf(targetItem))
                {
                    return DragModes.None;
                }
            }

            Point position = e.GetPosition(targetItem);
            double normalizedY = position.Y / targetItem.ActualHeight;
            if (normalizedY < this.InsertEffectRange)
            {
                if (this.ItemDragMode.HasFlag(DragModes.InsertFront))
                {
                    return DragModes.InsertFront;
                }
            }
            else if (normalizedY > 1.0 - this.InsertEffectRange)
            {
                if (this.ItemDragMode.HasFlag(DragModes.InsertBack))
                {
                    return DragModes.InsertBack;
                }
            }
            else
            {
                if (!this.ItemDragMode.HasFlag(DragModes.Drop))
                {
                    return DragModes.None;
                }
            }

            return DragModes.Drop;
        }

        private void showDragMask(DragEventArgs e)
        {
            TreeViewItem? sourceItem, targetItem;
            DragModes mode = this.checkDragMode(e, out sourceItem, out targetItem);
            if (DragModes.None == mode || targetItem is null)
            {
                e.Effects = DragDropEffects.None;
                this.hideDragMask();
                return;
            }

            if (sourceItem is not null)
            {
                DragEvent drag = new DragEvent(sourceItem, targetItem, mode);
                this.onDragOver?.Invoke(drag);
                if (drag.Canceled)
                {
                    e.Effects = DragDropEffects.None;
                    this.hideDragMask();
                    return;
                }
            }

            if (DragModes.InsertFront == mode)
            {
                this.showDragMask(targetItem, 0, DRAG_EFFECT_RATE);
                return;
            }
            else if (DragModes.InsertBack == mode)
            {
                this.showDragMask(targetItem, 1.0 - DRAG_EFFECT_RATE, DRAG_EFFECT_RATE);
                return;
            }
            else if (DragModes.Drop == mode)
            {
                this.showDragMask(targetItem, 0, 1.0);
                return;
            }

            this.hideDragMask();
        }

        private void showDragMask(TreeViewItem item, double normalizedPositionY, double normalizedHeight)
        {
            Border? mask = mDragEffectMask;
            UIElement? maskParent = mask?.Parent as UIElement;
            if (mask is null || maskParent is null)
            {
                this.hideDragMask();
                return;
            }

            FrameworkElement source = item.Label as FrameworkElement ?? item;
            Point tl = source.TranslatePoint(new Point(), maskParent);
            Point br = source.TranslatePoint(new Point(source.ActualWidth, source.ActualHeight), maskParent);
            double height = br.Y - tl.Y;
            mask.Width = br.X - tl.X;
            mask.Height = height * normalizedHeight;
            mask.Margin = new Thickness(tl.X, tl.Y + height * normalizedPositionY, 0, 0);
            mask.Visibility = Visibility.Visible;
        }

        private void hideDragMask()
        {
            if (mDragEffectMask is null)
            {
                return;
            }

            mDragEffectMask.Visibility = Visibility.Hidden;
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

            if (this.SelectedItem == item)
            {
                SetValue(SelectedItemPropertyKey, null);
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
            IEnumerable<TreeViewItem> selectedItems = this.SelectedItems;
            IEnumerable<TreeViewItem> intersectedItems = selectedItems.Intersect(mReportedSelectedItems);
            if (intersectedItems.Count() == mReportedSelectedItems.Count() && intersectedItems.Count() == selectedItems.Count())
            {
                return;
            }

            this.onSelectedItemsChange?.Invoke(selectedItems);
            mReportedSelectedItems = selectedItems.ToArray();
        }

        private bool checkClearSelectedItems(MouseButtonEventArgs e)
        {
            IInputElement? input = this.InputHitTest(e.GetPosition(this));
            if (input.FindAncestor<TreeViewItem>() is not null)
            {
                return false;
            }

            this.ClearAllSelections();
            return true;
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
            this.reportSelectedItemsChange();
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
            this.reportSelectedItemsChange();
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

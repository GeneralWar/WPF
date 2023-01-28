using General.WPF.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
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

        IEnumerable<IMultipleSelectionsItem> IMultipleSelectionsCollection.Items => this.Items.OfType<IMultipleSelectionsItem>();

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

        ITreeViewItemCollection? ITreeViewItemCollection.Parent => this.Parent as ITreeViewItemCollection;

        public IMultipleSelectionsItem? LastSelected { get; private set; }

        int ITreeViewItemCollection.SiblingIndex => -1;

        public delegate void OnDragEventDelegate(DragEvent e);
        public event OnDragEventDelegate? onDragOver = null;
        public event OnDragEventDelegate? onDrop = null;

        private bool mDragCancel = false;
        private Border? mDragEffectMask = null;

        private bool mIsAppending = false;

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
                        try
                        {
                            DragDropEffects result = DragDrop.DoDragDrop(item, item.ToDragData(), DragDropEffects.Move | DragDropEffects.Copy);
                            mDragCancel = DragDropEffects.None == result;
                        }
                        catch (Exception exception)
                        {
                            Tracer.Exception(exception);
                        }
                        finally
                        {
                            item.AllowDrop = true;
                        }

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

            TreeViewItem? sourceItem;
            ITreeViewItemCollection? targetItem;
            DragModes mode = this.checkDragMode(e, out sourceItem, out targetItem);
            if (DragModes.None == mode || targetItem is null)
            {
                e.Effects = DragDropEffects.None;
                this.hideDragMask();
                return;
            }

            DragEvent drag = new DragEvent(sourceItem ?? e.GetDragSource(), targetItem, mode);
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

        private DragModes checkDragMode(DragEventArgs e, out TreeViewItem? sourceItem, out ITreeViewItemCollection? targetItem)
        {
            sourceItem = e.GetDragSource() as TreeViewItem;
            targetItem = this.InputHitTest(e.GetPosition(this))?.FindAncestor<ITreeViewItemCollection>();
            if (targetItem is null)
            {
                return DragModes.None;
            }

            Control target = targetItem as Control ?? throw new InvalidOperationException();
            if (sourceItem is not null)
            {
                if (sourceItem == targetItem && sourceItem.Parent == targetItem || sourceItem.IsAncestorOf(target))
                {
                    return DragModes.None;
                }
            }

            Point position = e.GetPosition(target);
            double normalizedY = position.Y / target.ActualHeight;
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
            TreeViewItem? sourceItem;
            ITreeViewItemCollection? targetItem;
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

        private void showDragMask(ITreeViewItemCollection item, double normalizedPositionY, double normalizedHeight)
        {
            Border? mask = mDragEffectMask;
            UIElement? maskParent = mask?.Parent as UIElement;
            if (mask is null || maskParent is null || item is not TreeViewItem)
            {
                this.hideDragMask();
                return;
            }

            TreeViewItem treeItem = item as TreeViewItem ?? throw new InvalidCastException();
            FrameworkElement source = treeItem.Label as FrameworkElement ?? treeItem;
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
                this.LastSelected = selectable;
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

        private bool checkShouldSwitchDirection(IMultipleSelectionsItem from, IMultipleSelectionsItem to)
        {
            if (from == to)
            {
                return false;
            }

            string fromID = this.getPathID(from);
            string toID = this.getPathID(to);
            return string.Compare(fromID, toID) > 0;
        }

        private void selectItems(IMultipleSelectionsItem from, IMultipleSelectionsItem to)
        {
            this.append(from, false);

            if (from == to)
            {
                return;
            }

            if (this.checkShouldSwitchDirection(from, to))
            {
                this.selectItems(to, from);
                return;
            }

            if (from.Parent == to.Parent)
            {
                IEnumerator? enumerator = from.Parent.GetEnumerator();
                if (enumerator is null)
                {
                    return;
                }

                while (enumerator.MoveNext() && enumerator.Current != from) ;
                this.append(from, true);

                while (enumerator.MoveNext() && enumerator.Current != to)
                {
                    IMultipleSelectionsItem? i = enumerator.Current as IMultipleSelectionsItem;
                    if (i is not null)
                    {
                        this.append(i, true);
                    }
                }
                this.append(to, false);
            }
            else
            {
                this.enumerateTo(from, to, i => this.append(i, false));
            }

            mLastOperatedItem = to;
        }

        private void enumerateTo(IMultipleSelectionsItem from, IMultipleSelectionsItem to, Action<IMultipleSelectionsItem>? handler)
        {
            if (from == to)
            {
                return;
            }

            DependencyObject? fromObject = from as DependencyObject;
            DependencyObject? toObject = to as DependencyObject;
            if (fromObject is null || toObject is null)
            {
                return;
            }

            EnumerateTo enumerator = new EnumerateTo(fromObject, toObject);
            enumerator.Enumerate(i =>
            {
                IMultipleSelectionsItem? item = i as IMultipleSelectionsItem;
                if (item is not null)
                {
                    handler?.Invoke(item);
                }
            });
        }

        private string getPathID(IMultipleSelectionsItem item)
        {
            StringBuilder builder = new StringBuilder();
            FrameworkElement? element = item as FrameworkElement;
            while (element is not null && this != element)
            {
                builder.Append('_');
                builder.Append(element.GetSiblingIndex());
                element = element.Parent as FrameworkElement;
            }
            return builder.Remove(0, 1).ToString().Reverse();
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
            if (mIsAppending)
            {
                return;
            }

            if (1 == mSelectedItems.Count && mSelectedItems.First() == item)
            {
                return;
            }

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
            if (this.LastSelected == item)
            {
                return;
            }

            if (this.LastSelected is not null)
            {
                this.selectItems(this.LastSelected, item);
            }
        }

        void IMultipleSelectionsCollection.Append(IMultipleSelectionsItem item)
        {
            this.append(item, false);
        }

        private void append(IMultipleSelectionsItem item, bool recursively)
        {
            mIsAppending = true;
            mSelectedItems.Add(item);
            this.LastSelected = item;
            (item as FrameworkElement)?.Focus();
            mLastOperatedItem = item;
            item.IsSelected = true;
            this.reportSelectedItemsChange();
            mIsAppending = false;

            if (recursively)
            {
                DependencyObject? parent = item as DependencyObject;
                if (parent is not null)
                {
                    IEnumerator? enumerator = parent.GetEnumerator();
                    while (enumerator is not null && enumerator.MoveNext())
                    {
                        IMultipleSelectionsItem? child = enumerator.Current as IMultipleSelectionsItem;
                        if (child is not null)
                        {
                            this.append(child, recursively);
                        }
                    }
                }
            }
        }

        void IMultipleSelectionsCollection.Unselect(IMultipleSelectionsItem item)
        {
            if (mIsAppending)
            {
                return;
            }

            if (!mSelectedItems.Remove(item))
            {
                return;
            }

            item.IsSelected = false;
            if (this.SelectedItem == item)
            {
                this.setAsBaseSelected(mSelectedItems.FirstOrDefault());
            }

            mLastOperatedItem = item;
            if (this.LastSelected == item)
            {
                this.LastSelected = null;
            }

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

        public void ExpandAll()
        {
            this.expandAll(this.Items);
        }

        private void expandAll(ItemCollection items)
        {
            foreach (System.Windows.Controls.TreeViewItem item in items.OfType<System.Windows.Controls.TreeViewItem>())
            {
                item.IsExpanded = true;
                this.expandAll(item.Items);
            }
        }

        public void CollapseAll()
        {
            this.collapseAll(this.Items);
        }

        private void collapseAll(ItemCollection items)
        {
            foreach (System.Windows.Controls.TreeViewItem item in items.OfType<System.Windows.Controls.TreeViewItem>())
            {
                item.IsExpanded = false;
                this.expandAll(item.Items);
            }
        }
    }
}

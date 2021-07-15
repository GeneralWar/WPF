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

        public delegate void OnItemHeaderChange(TreeViewItem item);
        public event OnItemHeaderChange onItemHeaderChange = null;

        public delegate void OnSelectedItemsChange(IList items);
        public event OnSelectedItemsChange onSelectedItemsChange = null;

        private Style mItemStyle = null;
        private TreeViewItem mEditingItem = null;

        public Brush InactiveSelectionBackground { get; set; } = new SolidColorBrush(Color.FromArgb(128, 144, 144, 144));

        private List<TreeViewItem> mSelectedItems = new List<TreeViewItem>();
        public IList SelectedItems => mSelectedItems;

        public TreeView()
        {
            InitializeComponent();
            mItemStyle = this.FindResource("TreeViewItemStyle") as Style;
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

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

                item.Style = mItemStyle;
                item.LostFocus += this.onItemInputLostFocus;
            }
        }

        private void onInputBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (Key.Enter != e.Key)
            {
                return;
            }

            TextBox inputBox = sender as TextBox;
            if (null == inputBox)
            {
                return;
            }

            TreeViewItem item = inputBox.Tag as TreeViewItem;
            if (null == item)
            {
                return;
            }

            this.Commit(item);
        }

        private void onHeaderMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Grid)
            {
                return;
            }

            TreeViewItem item = (sender as Grid).Tag as TreeViewItem;
            if (null == item)
            {
                return;
            }

            if (item.IsSelected)
            {
                if (mSelectedItems.Count > 1)
                {
                    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    {
                        item.IsSelected = false;
                    }
                    else
                    {
                        this.ClearSelectedItems();
                        item.IsSelected = true;
                    }
                    e.Handled = true;
                    return;
                }
                else if (1 == mSelectedItems.Count && item == mSelectedItems[0])
                {
                    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    {
                        this.ClearSelectedItems();
                        e.Handled = true;
                        return;
                    }
                }

                if (item.IsFocused)
                {
                    e.Handled = true;
                    this.Edit(item);
                    return;
                }
            }
            //else if (MouseButton.Left == e.ChangedButton)
            //{
            //    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            //    {
            //        if (item.IsSelected || mSelectedItems.Contains(item))
            //        {
            //            this.unselect(item);
            //        }
            //        else
            //        {
            //            this.select(item);
            //        }
            //    }
            //    else
            //    {
            //        this.clearSelectedItems();
            //        item.IsSelected = true;
            //        this.select(item);
            //    }
            //    e.Handled = true;
            //}
        }

        /// <summary>
        /// Convert specific TreeViewItem into edit mode
        /// </summary>
        /// <param name="item">The TreeViewItem which want to edit</param>
        public void Edit(TreeViewItem item)
        {
            if (null != mEditingItem)
            {
                this.Commit(mEditingItem);
            }

            if (this != item.GetTreeViewOwner())
            {
                return;
            }

            mEditingItem = item;

            TextBox inputBox = item.Template?.FindName("InputBox", item) as TextBox;
            if (null == inputBox)
            {
                return;
            }

            inputBox.LostFocus += onItemInputLostFocus;
            inputBox.Visibility = Visibility.Visible;
            inputBox.SelectAll();
            inputBox.Focus();
        }

        private void onItemInputLostFocus(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = sender is TextBox ? (sender as TextBox).Tag as TreeViewItem : sender as TreeViewItem;
            if (null == item)
            {
                return;
            }

            //if (!item.IsSelected && sender is TreeViewItem && mSelectedItems.Contains(item))
            //{
            //    Border board = item.Template.FindName("TextBoard", item) as Border;
            //    board.Background = this.InactiveSelectionBackground;
            //    item.Foreground = SystemColors.InactiveSelectionHighlightTextBrush;
            //}

            IInputElement hitControl = item.InputHitTest(InputManager.Current.PrimaryMouseDevice.GetPosition(item));
            if (null != hitControl)
            {
                return;
            }

            this.Commit(item);
        }

        /// <summary>
        /// Commit the text in TextBox as the item's header
        /// </summary>
        /// <param name="item">The TreeViewItem which want to commit</param>
        public void Commit(TreeViewItem item)
        {
            //Trace.Assert(mEditingItem == item); // can occur when deleting item

            TextBox inputBox = item.Template?.FindName("InputBox", item) as TextBox;
            if (null == inputBox)
            {
                return;
            }

            inputBox.LostFocus -= onItemInputLostFocus;
            inputBox.Visibility = Visibility.Hidden;

            item.Header = inputBox.Text;
            this.onItemHeaderChange?.Invoke(item);
        }

        //private void select(TreeViewItem item)
        //{
        //    Border board = item.Template.FindName("TextBoard", item) as Border;
        //    board.Background = SystemColors.HighlightBrush;
        //    item.Foreground = SystemColors.HighlightTextBrush;
        //    mSelectedItems.Add(item);
        //}

        //private void unselect(TreeViewItem item)
        //{
        //    Border board = item.Template.FindName("TextBoard", item) as Border;
        //    board.Background = Brushes.Transparent;
        //    item.Foreground = SystemColors.ControlTextBrush;
        //    mSelectedItems.Remove(item);
        //}

        //private void clearSelectedItems()
        //{
        //    foreach (object i in mSelectedItems.ToArray())
        //    {
        //        TreeViewItem item = i as TreeViewItem;
        //        if (null == item)
        //        {
        //            continue;
        //        }
        //        this.unselect(item);
        //    }

        //    mSelectedItems.Clear();
        //}

        protected override void OnSelectedItemChanged(RoutedPropertyChangedEventArgs<object> e)
        {
            if (IsSelectionChangeActiveProperty == null)
            {
                return;
            }

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                object isSelectionChangeActive = IsSelectionChangeActiveProperty.GetValue(this, null);
                IsSelectionChangeActiveProperty.SetValue(this, true, null);

                if (e.NewValue is TreeViewItem)
                {
                    mSelectedItems.Add(e.NewValue as TreeViewItem);
                    mSelectedItems.ForEach(item => item.IsSelected = true);
                    this.reportSelectedItemsChange();
                }
                else if (null != e.OldValue && null == e.NewValue && e.OldValue is TreeViewItem)
                {
                    TreeViewItem item = e.OldValue as TreeViewItem;
                    if (!item.IsSelected)
                    {
                        mSelectedItems.Remove(item);
                        this.reportSelectedItemsChange();
                    }
                }

                IsSelectionChangeActiveProperty.SetValue(this, isSelectionChangeActive, null);
            }
            else
            {
                this.ClearSelectedItems();
                base.OnSelectedItemChanged(e);
                if (e.NewValue is TreeViewItem)
                {
                    mSelectedItems.Add(e.NewValue as TreeViewItem);
                }
                this.reportSelectedItemsChange();
            }
        }

        private void reportSelectedItemsChange()
        {
            this.onSelectedItemsChange?.Invoke(mSelectedItems);
        }

        public void ClearSelectedItems()
        {
            Array.ForEach(mSelectedItems.ToArray(), item => item.IsSelected = false);
            mSelectedItems.Clear();
            this.reportSelectedItemsChange();
        }
    }
}

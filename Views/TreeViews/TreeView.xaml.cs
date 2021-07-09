using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace General.WPF
{
    /// <summary>
    /// TreeView.xaml 的交互逻辑
    /// </summary>
    public partial class TreeView : System.Windows.Controls.TreeView
    {
        public delegate void OnItemHeaderChange(TreeViewItem item);
        public event OnItemHeaderChange onItemHeaderChange = null;

        private Style mItemStyle = null;
        private TreeViewItem mEditingItem = null;

        public Brush InactiveSelectionBackground { get; set; } = new SolidColorBrush(Color.FromArgb(128, 144, 144, 144));

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

            if (item.IsSelected && item.IsFocused)
            {
                e.Handled = true;
                this.Edit(item);
            }
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
            Trace.Assert(mEditingItem == item);

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
    }
}

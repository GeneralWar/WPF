using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace General.WPF
{
    /// <summary>
    /// TreeViewItem.xaml 的交互逻辑
    /// </summary>
    public partial class TreeViewItem : System.Windows.Controls.TreeViewItem, ITreeViewItemCollection
    {
        public delegate void OnItemHeaderChange(TreeViewItem item);
        public event OnItemHeaderChange? onItemHeaderChange = null;

        public delegate void OnItemsChange(object sender, NotifyCollectionChangedEventArgs e);
        public event OnItemsChange? onItemsChange = null;

        private Border? mTextBoard = null;
        private Border? mInputBoard = null;

        public TreeViewItem()
        {
            InitializeComponent();

            this.LostFocus += this.onItemInputLostFocus;
        }

        private void checkTempalteBoards()
        {
            mTextBoard ??= this.Template.FindName("TextBoard", this) as Border;
            mInputBoard ??= this.Template.FindName("InputBoard", this) as Border;
        }

        public bool IsHeaderArea(IInputElement element)
        {
            this.checkTempalteBoards();
            return element is TextBlock || element == mTextBoard || element == mInputBoard;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            this.onMouseDown(e);
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            this.onMouseDown(e);
        }

        private void onMouseDown(MouseButtonEventArgs e)
        {
            if (this.IsSelected && MouseButton.Left == e.ChangedButton && this.IsHeaderArea(this.InputHitTest(e.GetPosition(this))))
            {
                TreeView? root = this.GetTreeViewOwner();
                if (root is not null && 1 == root.SelectedItems.Count() && this == root.SelectedItems.ElementAt(0))
                {
                    this.Edit();
                    e.Handled = true;
                }
            }
        }

        private void onInputBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (Key.Enter != e.Key)
            {
                return;
            }

            TextBox? inputBox = sender as TextBox;
            if (inputBox is null)
            {
                return;
            }

            TreeViewItem? item = inputBox.Tag as TreeViewItem;
            if (item is null)
            {
                return;
            }

            this.Commit(item);
        }

        /// <summary>
        /// Convert specific TreeViewItem into edit mode
        /// </summary>
        /// <param name="item">The TreeViewItem which want to edit</param>
        public void Edit()
        {
            TextBox? inputBox = this.Template?.FindName("InputBox", this) as TextBox;
            if (inputBox is null)
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
            TreeViewItem? item = sender is TextBox ? (sender as TextBox)?.Tag as TreeViewItem : sender as TreeViewItem;
            if (item is null)
            {
                return;
            }

            //if (!item.IsSelected && sender is TreeViewItem && mSelectedItems.Contains(item))
            //{
            //    Border board = item.Template.FindName("TextBoard", item) as Border;
            //    board.Background = this.InactiveSelectionBackground;
            //    item.Foreground = SystemColors.InactiveSelectionHighlightTextBrush;
            //}

            IInputElement? hitControl = item.InputHitTest(InputManager.Current.PrimaryMouseDevice.GetPosition(item));
            if (hitControl is null)
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

            TextBox? inputBox = item.Template?.FindName("InputBox", item) as TextBox;
            if (inputBox is null)
            {
                return;
            }

            inputBox.LostFocus -= onItemInputLostFocus;
            inputBox.Visibility = Visibility.Hidden;

            item.Header = inputBox.Text;
            this.onItemHeaderChange?.Invoke(item);
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            this.onItemsChange?.Invoke(this, e);
        }
    }
}

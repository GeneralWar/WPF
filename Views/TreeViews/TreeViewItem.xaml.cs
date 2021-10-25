using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
        public delegate bool OnItemHeaderChange(TreeViewItem item, string oldName, string newName);
        public event OnItemHeaderChange? onItemHeaderChanging = null;

        public delegate void OnItemsChange(object sender, NotifyCollectionChangedEventArgs e);
        public event OnItemsChange? onItemsChange = null;

        private Border? mTextBoard = null;
        private Border? mInputBoard = null;

        private bool mIsEditing = false;

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
            FrameworkElement? hit = this.InputHitTest(e.GetPosition(this)) as FrameworkElement;
            TreeViewItem? ancestor = hit?.FindAncestor<TreeViewItem>();
            if (this != ancestor)
            {
                return;
            }

            if (this.IsSelected && MouseButton.Left == e.ChangedButton/* && this.IsHeaderArea(this.InputHitTest(e.GetPosition(this)))*/)
            {
                TreeView? root = this.GetTreeViewOwner();
                if (root is not null)
                {
                    if (root.IsOnlySelected(this))
                    {
                        this.Edit();
                        e.Handled = true;
                    }
                    else
                    {
                        root.Select(this);
                    }
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

            TreeViewItem? item = inputBox.FindAncestor<TreeViewItem>();
            if (this != item)
            {
                return;
            }

            this.Commit();
        }

        /// <summary>
        /// Convert specific TreeViewItem into edit mode
        /// </summary>
        /// <param name="item">The TreeViewItem which want to edit</param>
        public void Edit()
        {
            if (mIsEditing)
            {
                return;
            }

            TextBox? inputBox = this.Template?.FindName("InputBox", this) as TextBox;
            if (inputBox is null)
            {
                return;
            }

            mIsEditing = true;

            inputBox.LostFocus += onItemInputLostFocus;
            inputBox.Visibility = Visibility.Visible;
            inputBox.SelectAll();
            inputBox.Focus();
        }

        private void onItemInputLostFocus(object sender, RoutedEventArgs e)
        {
            TreeViewItem? item = (sender as FrameworkElement)?.FindAncestor<TreeViewItem>();
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

            FrameworkElement? hitControl = item.InputHitTest(InputManager.Current.PrimaryMouseDevice.GetPosition(item)) as FrameworkElement;
            if (hitControl is not null)
            {
                TreeViewItem? focusedItem = hitControl.FindAncestor<TreeViewItem>();
                if (this == focusedItem)
                {
                    return;
                }
            }

            FrameworkElement? currentFocused = FocusManager.GetFocusedElement(this.GetTopWindow()) as FrameworkElement;
            if (this.IsAncestorOf(currentFocused))
            {
                return;
            }

            this.Commit();
        }

        /// <summary>
        /// Commit the text in TextBox as the item's header
        /// </summary>
        /// <param name="item">The TreeViewItem which want to commit</param>
        public void Commit()
        {
            //Trace.Assert(mEditingItem == item); // can occur when deleting item

            if (!mIsEditing)
            {
                return;
            }

            TextBox? inputBox = this.Template?.FindName("InputBox", this) as TextBox;
            if (inputBox is not null)
            {
                inputBox.LostFocus -= onItemInputLostFocus;
                inputBox.Visibility = Visibility.Hidden;

                string currentText = this.Header as string ?? "";
                string targetText = inputBox.Text;
                if (currentText != targetText)
                {
                    bool changeResult = true;
                    try
                    {
                        changeResult = this.onItemHeaderChanging?.Invoke(this, currentText, targetText) ?? true; // if no handler, default is true
                    }
                    catch (Exception e)
                    {
                        Trace.Assert(false, e.ToString());
                        changeResult = false;
                    }
                    finally
                    {
                        if (changeResult)
                        {
                            this.Header = targetText;
                        }
                        else
                        {
                            inputBox.Text = currentText;
                        }
                    }                    
                }
            }

            mIsEditing = false;
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            this.onItemsChange?.Invoke(this, e);
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == IsSelectedProperty)
            {
                this.Select((bool)e.NewValue);
            }
        }

        private void Select(bool isSelected)
        {
            TreeView? tree = this.FindAncestor<TreeView>();
            if (tree is null)
            {
                return;
            }

            if (isSelected)
            {
                tree.Select(this);
            }
            else
            {
                tree.Unselect(this);
                if (mIsEditing)
                {
                    this.Commit();
                }
            }
        }
    }
}

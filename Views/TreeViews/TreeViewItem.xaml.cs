using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace General.WPF
{
    /// <summary>
    /// TreeViewItem.xaml 的交互逻辑
    /// </summary>
    public partial class TreeViewItem : System.Windows.Controls.TreeViewItem, ITreeViewItemCollection, IMultipleSelectionsItem
    {
        static public DependencyProperty IsEditableProperty = DependencyProperty.Register(nameof(IsEditable), typeof(bool), typeof(TreeViewItem), new PropertyMetadata(true));

        public delegate bool OnItemHeaderChange(TreeViewItem item, string oldName, string newName);
        public event OnItemHeaderChange? onItemHeaderChanging = null;

        public delegate void OnItemsChange(object sender, NotifyCollectionChangedEventArgs e);
        public event OnItemsChange? onItemsChange = null;

        private Border? mTextBoard = null;
        private Border? mInputBoard = null;

        private bool mIsEditing = false;
        private bool mCanEdit = false;

        public bool IsEditable { get { return (bool)GetValue(IsEditableProperty); } set { SetValue(IsEditableProperty, value); } }

        public UIElement? Label => (this.Template.FindName("TextBoard", this) as Border)?.Child as UIElement;

        private IMultipleSelectionsCollection? mCollection = null;
        private IMultipleSelectionsCollection Collection => mCollection ??= this.FindAncestor<IMultipleSelectionsCollection>() ?? throw new NullReferenceException();
        IMultipleSelectionsCollection IMultipleSelectionsItem.Collection => this.Collection;

        private string mHeader = "";

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

        //public bool IsHeaderArea(IInputElement element)
        //{
        //    this.checkTempalteBoards();
        //    return element is TextBlock || element == mTextBoard || element == mInputBoard;
        //}

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.checkTempalteBoards();
        }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);
            mCollection = (this.Parent as IMultipleSelectionsCollection) ?? (this.Parent as IMultipleSelectionsItem)?.Collection;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            TreeViewItem? item = (e.Source as FrameworkElement)?.FindAncestor<TreeViewItem>();
            if (this != item)
            {
                return;
            }

            mCanEdit = (this as IMultipleSelectionsItem).IsOnlySelected();

            if (e.IsShiftDown())
            {
                this.Collection.SelectTo(this);
                return;
            }

            if (e.IsControlDown())
            {
                this.Collection.Append(this);
                return;
            }

            if (MouseButton.Left == e.ChangedButton)
            {
                if (!this.IsSelected || !(this as IMultipleSelectionsItem).IsOnlySelected())
                {
                    this.Collection.Select(this);
                }
                return;
            }
            else if (MouseButton.Right == e.ChangedButton)
            {
                return;
            }

            Trace.Assert(false, "Unexpected condition");
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            if (MouseButton.Left == e.ChangedButton && (this as IMultipleSelectionsItem).IsOnlySelected())
            {
                if (mCanEdit)
                {
                    this.Edit();
                    e.Handled = true;
                }
                mCanEdit = false;
            }
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            mCanEdit = false;
        }

        private void onInputBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (Key.Enter != e.Key)
            {
                if (Key.Escape == e.Key)
                {
                    this.Cancel();
                }
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
            if (!this.IsEditable)
            {
                return;
            }

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

            mHeader = this.Header as string ?? "";

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

            Window? window = this.GetTopWindow();
            if (window is null) // assumed to be deleting
            {
                return;
            }

            FrameworkElement? currentFocused = FocusManager.GetFocusedElement(window) as FrameworkElement;
            if (currentFocused is null || this.IsAncestorOf(currentFocused)) // it may be null when exiting the application
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

                string currentText = mHeader;
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

        private void Cancel()
        {
            TextBox? inputBox = this.Template?.FindName("InputBox", this) as TextBox;
            if (inputBox is not null)
            {
                inputBox.Text = mHeader;
                inputBox.LostFocus -= onItemInputLostFocus;
                inputBox.Visibility = Visibility.Hidden;
            }
            this.Header = mHeader;
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
            if (isSelected)
            {
                if (!this.Collection.SelectedItems.Contains(this))
                {
                    this.Collection.Append(this);
                }
            }
            //else
            //{
            //    this.Collection.Unselect(this);
            //    if (mIsEditing)
            //    {
            //        this.Commit();
            //    }
            //}
        }
    }
}

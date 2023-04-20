using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace General.WPF
{
    /// <summary>
    /// TreeViewItem.xaml 的交互逻辑
    /// </summary>
    public partial class TreeViewItem : System.Windows.Controls.TreeViewItem, ITreeViewItemCollection, IMultipleSelectionsItem
    {
        static public DependencyProperty IsEditableProperty = DependencyProperty.Register(nameof(IsEditable), typeof(bool), typeof(TreeViewItem), new PropertyMetadata(true));

        private class EditToken
        {
            private bool mIsCanceled = false;
            public bool IsCanceled => mIsCanceled;

            public void Cancel()
            {
                mIsCanceled = true;
            }
        }

        public delegate bool OnItemHeaderChange(TreeViewItem item, string oldName, string newName);
        public event OnItemHeaderChange? OnItemHeaderChanging = null;

        public delegate void OnItemsChange(object sender, NotifyCollectionChangedEventArgs e);
        public event OnItemsChange? onItemsChange = null;

        private Border? mTextBoard = null;
        private Brush? mTextBoardBackground = null;
        private Border? mInputBoard = null;

        private bool mIsEditing = false;
        private bool mWaitForEditing = false;
        private bool mCanEdit = false;
        private EditToken mCancelToken = new EditToken();

        public bool IsEditable { get { return (bool)GetValue(IsEditableProperty); } set { SetValue(IsEditableProperty, value); } }

        internal Border? Label => mTextBoard;

        private IMultipleSelectionsCollection? mCollection = null;
        private IMultipleSelectionsCollection Collection => mCollection ??= this.FindAncestor<IMultipleSelectionsCollection>() ?? throw new NullReferenceException();
        IMultipleSelectionsCollection IMultipleSelectionsItem.Collection => this.Collection;

        ITreeViewItemCollection? ITreeViewItemCollection.Parent => this.Parent as ITreeViewItemCollection;

        int ITreeViewItemCollection.SiblingIndex => this.GetSiblingIndex();
        int IMultipleSelectionsItem.SiblingIndex => this.GetSiblingIndex();

        private string mHeader = "";

        public event Action<TreeViewItem>? OnEditConfirm = null;
        public event Action<TreeViewItem>? OnEditCancel = null;

        public TreeViewItem()
        {
            InitializeComponent();

            this.LostFocus += this.onItemInputLostFocus;
        }

        private void checkTempalteBoards()
        {
            mTextBoard ??= this.Template.FindName("TextBoard", this) as Border;
            mInputBoard ??= this.Template.FindName("InputBoard", this) as Border;
            if (mTextBoardBackground is not null)
            {
                this.updateBackgroudColor(mTextBoardBackground);
            }
        }

        private bool hitInSelf(Point position)
        {
            return this != this.InputHitTest(position).FindAncestor<TreeViewItem>();
        }

        private bool hitInHeader(Point position)
        {
            IInputElement element = this.InputHitTest(position);
            return mTextBoard == element || mInputBoard == element;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.checkTempalteBoards();
            if (mWaitForEditing)
            {
                this.Edit();
            }
        }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);
            mCollection = (this.Parent as IMultipleSelectionsCollection) ?? (this.Parent as IMultipleSelectionsItem)?.Collection;
            this.AllowDrop = this.GetTreeViewOwner()?.AllowItemDrag ?? false;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            this.updateColor();
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            this.updateColor();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            if (MouseButton.Left != e.ChangedButton)
            {
                return;
            }

            TreeViewItem? item = (e.Source as FrameworkElement)?.FindAncestor<TreeViewItem>();
            if (this != item)
            {
                return;
            }

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

            if (!this.IsSelected)
            {
                this.Collection.Select(this);
                return;
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            if (this.hitInSelf(e.GetPosition(this)))
            {
                return;
            }

            if (!(this as IMultipleSelectionsItem).IsOnlySelected())
            {
                if (!e.IsShiftDown() && !e.IsControlDown())
                {
                    this.Collection.Select(this);
                }
                mCanEdit = false;
                return;
            }

            if (MouseButton.Left != e.ChangedButton)
            {
                return;
            }

            if (mCanEdit)
            {
                string header = this.Header as string ?? "";
                Tracer.Log($"{nameof(TreeViewItem)}.{nameof(OnMouseUp)}: try to cancel edit of {header}");
                mCancelToken.Cancel();

                mCancelToken = new EditToken();
                ThreadPool.QueueUserWorkItem(o =>
                {
                    EditToken token = o as EditToken ?? throw new InvalidCastException();
                    Thread.Sleep(500);

                    if (!token.IsCanceled)
                    {
                        Tracer.Log($"{nameof(TreeViewItem)}: try to edit {header}");
                        this.Dispatcher.Invoke(this.Edit);
                    }
                }, mCancelToken);
                mCanEdit = false;
                Tracer.Log($"{nameof(TreeViewItem)}: try to edit {header} after 500ms");
            }
            else
            {
                mCanEdit = true;
            }
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            if (this.hitInHeader(e.GetPosition(this)))
            {
                if (e.IsControlDown())
                {
                    if (this.IsExpanded)
                    {
                        this.CollapseSubtree();
                    }
                    else
                    {
                        this.ExpandSubtree();
                    }
                }
                else
                {
                    this.IsExpanded = !this.IsExpanded;
                }
                e.Handled = true;
            }
        }

        protected override void OnPreviewMouseDoubleClick(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDoubleClick(e);

            if (MouseButton.Left != e.ChangedButton)
            {
                return;
            }

            //e.Handled = true;
            mCanEdit = false;
            mCancelToken.Cancel();
            Trace.WriteLine($"{nameof(TreeViewItem)}.{nameof(OnPreviewMouseDoubleClick)}: try to cancel edit of {this.Header}");
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
                mWaitForEditing = true;
                return;
            }

            mIsEditing = true;

            mHeader = this.Header as string ?? "";

            inputBox.LostFocus += onItemInputLostFocus;
            inputBox.Visibility = Visibility.Visible;
            inputBox.Text = mHeader;
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
            if (currentFocused is null || this == currentFocused) // it may be null when exiting the application
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

            Tracer.Log($"{nameof(TreeViewItem)}: try to commit {this.Header}");

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
                        changeResult = this.OnItemHeaderChanging?.Invoke(this, currentText, targetText) ?? true; // if no handler, default is true
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

            this.OnEditConfirm?.Invoke(this);
        }

        private void Cancel()
        {
            Tracer.Log($"{nameof(TreeViewItem)}.{nameof(Cancel)}: try to cancel edit of {this.Header}");

            TextBox? inputBox = this.Template?.FindName("InputBox", this) as TextBox;
            if (inputBox is not null)
            {
                inputBox.Text = mHeader;
                inputBox.LostFocus -= onItemInputLostFocus;
                inputBox.Visibility = Visibility.Hidden;
            }
            this.Header = mHeader;
            mIsEditing = false;

            this.OnEditCancel?.Invoke(this);
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
                if ((bool)e.NewValue)
                {
                    this.Select(false);
                }
                else
                {
                    this.Collection.Unselect(this);
                    mCanEdit = false;
                }
            }

            if (e.Property == IsSelectedProperty || e.Property == IsSelectionActiveProperty || e.Property == IsMouseOverProperty || e.Property == IsEnabledProperty)
            {
                this.updateColor();
            }
        }

        /// <summary>
        /// Select the item
        /// </summary>
        /// <param name="append">Determine if clear previous selections (Only selection or multiple selections)</param>
        public void Select(bool append)
        {
            if (append)
            {
                this.Collection.Append(this);
            }
            else
            {
                this.Collection.Select(this);
            }
        }

        private void updateBackgroudColor(Brush brush)
        {
            mTextBoardBackground = brush;
            if (mTextBoard is not null)
            {
                mTextBoard.Background = brush;
            }
        }

        private void updateForegroundColor(Brush brush)
        {
            this.Foreground = brush;
        }

        private void updateColor()
        {
            if (this.IsSelected)
            {
                if (this.IsSelectionActive)
                {
                    this.updateBackgroudColor(SystemColors.HighlightBrush);
                    this.updateForegroundColor(SystemColors.HighlightTextBrush);
                }
                else
                {
                    this.updateBackgroudColor(SystemColors.InactiveSelectionHighlightBrush);
                    this.updateForegroundColor(SystemColors.InactiveSelectionHighlightTextBrush);
                }
                return;
            }

            if (!this.IsEnabled)
            {
                this.updateForegroundColor(SystemColors.GrayTextBrush);
                return;
            }

            if (this.IsMouseOver) // IsMouseDirectlyOver is weird
            {
                if (this == Mouse.DirectlyOver || this == Mouse.DirectlyOver.FindAncestor<TreeViewItem>())
                {
                    this.updateBackgroudColor(this.Resources["TreeViewItem.Backgroud.MouseOver"] as SolidColorBrush ?? SystemColors.InactiveSelectionHighlightBrush);
                    return;
                }
            }

            this.updateBackgroudColor(Brushes.Transparent);
            this.updateForegroundColor(SystemColors.ControlTextBrush);
        }

        public override string ToString()
        {
            return $"{this.GetType().FullName} {this.Header}, Items.Count:{this.Items.Count}";
        }
    }
}

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
        public bool IsEditable { get { return (bool)GetValue(IsEditableProperty); } set { SetValue(IsEditableProperty, value); } }

        static public DependencyProperty InputVisibleProperty = DependencyProperty.Register(nameof(InputVisible), typeof(bool), typeof(TreeViewItem), new PropertyMetadata(false));
        public bool InputVisible { get { return (bool)GetValue(InputVisibleProperty); } set { SetValue(InputVisibleProperty, value); } }

        static public DependencyProperty InputTextProperty = DependencyProperty.Register(nameof(InputText), typeof(string), typeof(TreeViewItem), new PropertyMetadata(""));
        public string InputText { get { return (string)GetValue(InputTextProperty) ?? ""; } set { SetValue(InputTextProperty, value); } }

        static public DependencyProperty AllowItemsDragProperty = DependencyProperty.Register(nameof(AllowItemsDrag), typeof(bool), typeof(TreeViewItem), new PropertyMetadata(true));
        public bool AllowItemsDrag { get { return (bool)GetValue(AllowItemsDragProperty); } set { SetValue(AllowItemsDragProperty, value); } }

        static public DependencyProperty ShowConnectingLinesAmongItemsProperty = DependencyProperty.Register(nameof(ShowConnectingLinesAmongItems), typeof(bool), typeof(TreeViewItem), new PropertyMetadata(false));
        public bool ShowConnectingLinesAmongItems { get { return (bool)GetValue(ShowConnectingLinesAmongItemsProperty); } set { SetValue(ShowConnectingLinesAmongItemsProperty, value); } }

        static public DependencyProperty ConnectingLineColorProperty = DependencyProperty.Register(nameof(ConnectingLineColor), typeof(Brush), typeof(TreeViewItem), new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0xdc, 0xdc, 0xdc))));
        public Brush ConnectingLineColor { get { return (Brush)GetValue(ConnectingLineColorProperty); } set { SetValue(ConnectingLineColorProperty, value); } }

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

        internal delegate void OnItemsChangeDelegate(object sender, NotifyCollectionChangedEventArgs e);
        internal event OnItemsChangeDelegate? OnItemsChange = null;

        private bool mIsEditing = false;
        private bool mCanEdit = false;
        private EditToken mEditCancelToken = new EditToken();

        private IMultipleSelectionsCollection? mCollection = null;
        private IMultipleSelectionsCollection? Collection => mCollection ??= this.FindAncestor<IMultipleSelectionsCollection>()/* ?? throw new NullReferenceException()*/;
        IMultipleSelectionsCollection? IMultipleSelectionsItem.Collection => this.Collection;

        ITreeViewItemCollection? ITreeViewItemCollection.Parent => this.Parent as ITreeViewItemCollection;

        int ITreeViewItemCollection.SiblingIndex => this.GetSiblingIndex();
        int IMultipleSelectionsItem.SiblingIndex => this.GetSiblingIndex();

        public event Action<TreeViewItem>? OnEditConfirm = null;
        public event Action<TreeViewItem>? OnEditCancel = null;

        public TreeViewItem()
        {
            InitializeComponent();
            this.LostFocus += this.onInputLostFocus;
        }

        private bool hitInSelf(object source)
        {
            return this == (source as UIElement)?.FindAncestor<TreeViewItem>();
        }

        protected override void OnVisualParentChanged(DependencyObject oldParent)
        {
            base.OnVisualParentChanged(oldParent);
            mCollection = (this.Parent as IMultipleSelectionsCollection) ?? (this.Parent as IMultipleSelectionsItem)?.Collection;
            this.AllowDrop = this.FindAncestor<TreeViewItem>()?.AllowDrop ?? this.GetTreeViewOwner()?.AllowDrop ?? this.AllowDrop;
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

            if (!this.hitInSelf(e.OriginalSource))
            {
                return;
            }

            e.Handled = true;

            if (e.IsShiftDown())
            {
                this.Collection?.SelectTo(this);
                return;
            }

            if (e.IsControlDown())
            {
                this.Collection?.Append(this);
                return;
            }

            if (!this.IsSelected)
            {
                this.Collection?.Select(this);
                return;
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            if (MouseButton.Left != e.ChangedButton)
            {
                return;
            }

            if (!this.hitInSelf(e.OriginalSource))
            {
                return;
            }

            e.Handled = true;

            if (!(this as IMultipleSelectionsItem).IsOnlySelected())
            {
                if (!e.IsShiftDown() && !e.IsControlDown())
                {
                    this.Collection?.Select(this);
                }
                mCanEdit = false;
                return;
            }

            if (mCanEdit)
            {
                string header = this.Header as string ?? "";
                Tracer.Log($"{nameof(TreeViewItem)}.{nameof(OnMouseUp)}: try to cancel edit of {header}");
                mEditCancelToken.Cancel();

                mEditCancelToken = new EditToken();
                ThreadPool.QueueUserWorkItem(o =>
                {
                    EditToken token = o as EditToken ?? throw new InvalidCastException();
                    Thread.Sleep(500);

                    if (!token.IsCanceled)
                    {
                        Tracer.Log($"{nameof(TreeViewItem)}: try to edit {header}");
                        this.Dispatcher.InvokeAsync(this.Edit);
                    }
                }, mEditCancelToken);
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

            mEditCancelToken.Cancel();

            if (MouseButton.Left != e.ChangedButton)
            {
                return;
            }

            if (!this.hitInSelf(e.OriginalSource))
            {
                return;
            }

            e.Handled = true;

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
        }

        protected override void OnPreviewMouseDoubleClick(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDoubleClick(e);

            if (MouseButton.Left != e.ChangedButton)
            {
                return;
            }

            mCanEdit = false;
            mEditCancelToken.Cancel();
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

            mIsEditing = this.InputVisible = true;
            this.InputText = this.Header as string ?? "";
        }

        private void onInputVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            TextBox? input = sender as TextBox;
            if (input is null)
            {
                return;
            }

            if (e.NewValue.Equals(Visibility.Visible) || e.NewValue.Equals(true))
            {
                input.SelectAll();
                input.Focus();
            }
        }

        private void onInputLostFocus(object sender, RoutedEventArgs e)
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
            if (currentFocused is null || this == currentFocused || currentFocused.FindAncestor<TreeViewItem>() == this) // it may be null when exiting the application
            {
                return;
            }

            this.Commit();
        }

        private void onLostFocus(object sender, RoutedEventArgs e)
        {
            mEditCancelToken.Cancel();
            this.onInputLostFocus(sender, e);
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
                inputBox.LostFocus -= onInputLostFocus;
                inputBox.Visibility = Visibility.Hidden;

                string currentText = this.InputText;
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

            mIsEditing = this.InputVisible = false;

            this.OnEditConfirm?.Invoke(this);
        }

        private void Cancel()
        {
            Tracer.Log($"{nameof(TreeViewItem)}.{nameof(Cancel)}: try to cancel edit of {this.Header}");

            this.InputText = this.Header as string ?? "";
            mIsEditing = this.InputVisible = false;

            this.OnEditCancel?.Invoke(this);
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);
            this.OnItemsChange?.Invoke(this, e);

            if (e.NewItems is not null)
            {
                foreach (TreeViewItem item in e.NewItems.OfType<TreeViewItem>())
                {
                    item.ConnectingLineColor = this.ConnectingLineColor;
                    item.ShowConnectingLinesAmongItems = this.ShowConnectingLinesAmongItems;
                }
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == IsSelectedProperty)
            {
                if ((bool)e.NewValue)
                {
                    if (!this.Select(false))
                    {
                        return;
                    }
                }
                else
                {
                    this.Collection?.Unselect(this);
                    mCanEdit = false;
                }
            }
            else if (ShowConnectingLinesAmongItemsProperty == e.Property)
            {
                bool value = (bool)e.NewValue;
                this.Style = this.FindResource(value ? "TreeViewItemAddMinusStyle" : "TreeViewItemTriangleStyle") as Style;
                this.Items.OfType<TreeViewItem>().ForEach(item => item.ShowConnectingLinesAmongItems = value);
            }
            else if (ConnectingLineColorProperty == e.Property)
            {
                Brush value = (Brush)e.NewValue;
                this.Items.OfType<TreeViewItem>().ForEach(item => item.ConnectingLineColor = value);
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
        public bool Select(bool append)
        {
            if (append)
            {
                this.Collection?.Append(this);
                return true;
            }
            else
            {
                return this.Collection?.Select(this) ?? false;
            }
        }

        private void updateBackgroudColor(Brush brush)
        {
            this.Background = brush;
            //mTextBoardBackground = brush;
            //if (mTextBoard is not null)
            //{
            //    mTextBoard.Background = brush;
            //}
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

    public class TreeViewItem<DataType> : TreeViewItem where DataType : class
    {
        public DataType? Data { get; set; }
        private IComponentHostedData? HostedData { get; set; }

        public TreeViewItem() : this(null) { }

        public TreeViewItem(DataType? data)
        {
            this.Data = data;

            IComponentHostedData? record = this.HostedData;
            if (record is not null)
            {
                this.Loaded -= record.onLoad;
                this.Unloaded -= record.onUnload;
            }

            this.HostedData = data as IComponentHostedData;
            if (this.HostedData is not null)
            {
                this.Loaded += this.HostedData.onLoad;
                this.Unloaded += this.HostedData.onUnload;
            }
        }

        private void registerHostedData(IComponentHostedData data)
        {
            this.Loaded += data.onLoad;
            this.Unloaded += data.onUnload;
        }

        private void unregisterHostedData(IComponentHostedData data)
        {
            this.Loaded -= data.onLoad;
            this.Unloaded -= data.onUnload;
        }

        public override string ToString() => $"{nameof(TreeViewItem)}, Data: {this.Data?.ToString() ?? "(null)"}";
    }
}

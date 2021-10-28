using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace General.WPF
{
    /// <summary>
    /// EditableText.xaml 的交互逻辑
    /// </summary>
    public partial class EditableLabel : UserControl
    {
        public string Text { get { return GetValue(TextProperty) as string ?? ""; } set { SetValue(TextProperty, value); } }

        public bool IsEditing { get { return (bool)GetValue(IsEditingProperty); } set { SetValue(IsEditingProperty, value); } }
        public bool IsSelected { get { return (bool)GetValue(IsSelectedProperty); } set { SetValue(IsSelectedProperty, value); } }

        public delegate void OnEditableLabelChange(EditableLabel label);
        public event OnEditableLabelChange? onEditCancel = null;
        public event OnEditableLabelChange? onEditFinish = null;

        public delegate bool OnEditableLabelChanging(EditableLabel label, string newText);
        public event OnEditableLabelChanging? onLabelChanging = null;

        public EditState State { get { return (EditState)GetValue(StateProperty); } private set { SetValue(StateProperty, value); } }
        public delegate void OnStateChange(StateChangingEvent e);
        public event OnStateChange? onStateChanging = null;

        private bool mCanEdit = false;
        private bool mIsCommiting = false;
        private bool mIsEditCanceled = false;

        private string? mPlaceHolder = null;
        private string? mText = null;

        public EditableLabel()
        {
            InitializeComponent();

            this.LostFocus += this.onLostFocus;

            TextBox? input = this.Template.FindName("InputBox", this) as TextBox;
            if (input is not null)
            {
                input.LostFocus += this.onLostFocus;
            }
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            mCanEdit = this.IsSelected;
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            if (MouseButton.Left == e.ChangedButton && this.IsSelected)
            {
                this.IsEditing = mCanEdit;
                e.Handled = true;
                mCanEdit = false;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (this.IsEditing)
            {
                this.internalBeginEditing();
            }
        }

        public void BeginEditing()
        {
            this.IsEditing = true;
        }

        public void BeginEditing(string placeHolder)
        {
            mPlaceHolder = placeHolder;
            this.IsEditing = true;
        }

        private bool internalBeginEditing()
        {
            if (this.onStateChanging is not null)
            {
                StateChangingEvent e = new StateChangingEvent(this.State, EditState.Editing);
                this.onStateChanging.Invoke(e);
                if (e.Canceled)
                {
                    return false;
                }
            }

            TextBox? input = this.Template.FindName("InputBox", this) as TextBox;
            if (input is null)
            {
                return true; // Template not applied, assumed to be a new created label with editing state
            }

            if (!string.IsNullOrWhiteSpace(mPlaceHolder))
            {
                input.Text = mPlaceHolder;
                mPlaceHolder = null;
            }
            mText = this.Text;
            input.SelectAll();
            input.Focus();
            this.State = EditState.Editing;
            return true;
        }

        private void Commit()
        {
            if (mIsCommiting)
            {
                return;
            }

            mIsCommiting = true;

            this.internal_commit();

            mIsEditCanceled = false;
            this.IsEditing = false;
            mIsCommiting = false;

            this.State = this.IsSelected ? EditState.Selected : EditState.Normal;
        }

        private void internal_commit()
        {
            TextBox? input = this.Template.FindName("InputBox", this) as TextBox;
            if (input is null)
            {
                return;
            }

            string oldText = mText ?? "";
            string newText = input.Text;
            if (!mIsEditCanceled && oldText == newText)
            {
                return;
            }

            if (!mIsEditCanceled && (this.onLabelChanging?.Invoke(this, newText) ?? true))
            {
                if (oldText != newText)
                {
                    this.Text = newText;
                    this.onEditFinish?.Invoke(this);
                }
            }
            else
            {
                input.Text = oldText;
                this.onEditCancel?.Invoke(this);
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == IsEditingProperty)
            {
                bool isEditing = (bool)e.NewValue;
                if (!isEditing || !this.internalBeginEditing())
                {
                    this.Commit();
                }
            }
            else if (e.Property == IsSelectedProperty)
            {
                bool isSelected = (bool)e.NewValue;
                if (!isSelected)
                {
                    this.IsEditing = false;
                }
                this.State = IsSelected ? EditState.Selected : EditState.Normal;
            }
        }

        protected void onLostFocus(object sender, RoutedEventArgs e)
        {
            this.IsEditing = false;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (Key.Escape == e.Key)
            {
                mIsEditCanceled = true;
                this.IsEditing = false;
                return;
            }

            if (Key.Enter == e.Key)
            {
                this.IsEditing = false;
                return;
            }
        }
    }
}

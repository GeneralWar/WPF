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
        static public readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(EditableLabel));
        static public readonly DependencyProperty IsEditingProperty = DependencyProperty.Register("IsEditing", typeof(bool), typeof(EditableLabel));
        static public readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(EditableLabel));

        public string Text { get { return GetValue(TextProperty) as string ?? ""; } set { SetValue(TextProperty, value); } }

        public bool IsEditing { get { return (bool)GetValue(IsEditingProperty); } set { SetValue(IsEditingProperty, value); } }
        public bool IsSelected { get { return (bool)GetValue(IsSelectedProperty); } set { SetValue(IsSelectedProperty, value); } }

        public delegate void OnEditableLabelChange(EditableLabel label);
        public event OnEditableLabelChange? onEditCancel = null;
        public event OnEditableLabelChange? onEditFinish = null;

        public delegate bool OnEditableLabelChanging(EditableLabel label, string newText);
        public event OnEditableLabelChanging? onLabelChanging = null;

        private bool mIsEditCanceled = false;

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

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            if (MouseButton.Left == e.ChangedButton && this.IsSelected)
            {
                this.IsEditing = true;
                e.Handled = true;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (this.IsEditing)
            {
                this.beginEditing();
            }
        }

        private void beginEditing()
        {
            TextBox? input = this.Template.FindName("InputBox", this) as TextBox;
            if (input is null)
            {
                return;
            }

            input.SelectAll();
            input.Focus();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == IsEditingProperty)
            {
                if (e.NewValue is bool)
                {
                    bool isEditing = (bool)e.NewValue;
                    if (isEditing)
                    {
                        this.beginEditing();
                    }
                    else
                    {
                        var finder = this.Template.FindName("InputBox", this);
                        TextBox? input = this.Template.FindName("InputBox", this) as TextBox;
                        if (input is null)
                        {
                            return;
                        }

                        string newText = input.Text;
                        if (!mIsEditCanceled && (this.onLabelChanging?.Invoke(this, newText) ?? true))
                        {
                            this.Text = newText;
                            this.onEditFinish?.Invoke(this);
                        }
                        else
                        {
                            input.Text = this.Text;
                            this.onEditCancel?.Invoke(this);
                        }
                        mIsEditCanceled = false;
                    }
                }
            }

            if (e.Property == IsSelectedProperty && e.NewValue is bool && !(bool)e.NewValue)
            {
                this.IsEditing = false;
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

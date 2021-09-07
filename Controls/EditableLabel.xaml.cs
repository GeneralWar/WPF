using System;
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
        static public readonly DependencyProperty IsEditingProperty = DependencyProperty.Register("IsEditing", typeof(bool), typeof(EditableLabel));
        static public readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(EditableLabel));

        public string Text { get; set; }

        public bool IsEditing { get { return (bool)GetValue(IsEditingProperty); } set { SetValue(IsEditingProperty, value); } }
        public bool IsSelected { get { return (bool)GetValue(IsSelectedProperty); } set { SetValue(IsSelectedProperty, value); } }

        public delegate void OnEditableLabelChange(EditableLabel label);
        public event OnEditableLabelChange onEditCancel = null;
        public event OnEditableLabelChange onEditFinish = null;

        private bool mIsEditCanceled = false;

        public EditableLabel()
        {
            InitializeComponent();

            this.LostFocus += this.onLostFocus;

            TextBox input = this.Template.FindName("InputBox", this) as TextBox;
            if (null != input)
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
            }
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
                        object input = this.Template.FindName("InputBox", this);
                        (input as TextBox)?.SelectAll();
                        (input as TextBox)?.Focus();
                    }
                    else
                    {
                        if (mIsEditCanceled)
                        {
                            this.onEditCancel?.Invoke(this);
                        }
                        else
                        {
                            this.onEditFinish?.Invoke(this);
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

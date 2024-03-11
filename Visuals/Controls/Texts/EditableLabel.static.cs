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
        static public readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(EditableLabel));
        static public readonly DependencyProperty IsEditingProperty = DependencyProperty.Register(nameof(IsEditing), typeof(bool), typeof(EditableLabel));
        static public readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(nameof(IsSelected), typeof(bool), typeof(EditableLabel));

        public enum EditState
        {
            Normal,
            Selected,
            Editing,
        }

        public class StateChangingEvent : UIChangingEvent
        {
            public EditState OldState { get; private set; }
            public EditState NewState { get; private set; }

            public StateChangingEvent(EditState oldState, EditState newState)
            {
                this.OldState = oldState;
                this.NewState = newState;
            }
        }

        static public readonly DependencyProperty StateProperty = DependencyProperty.Register(nameof(State), typeof(EditState), typeof(EditableLabel));
    }
}

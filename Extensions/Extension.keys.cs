using System.Windows.Input;

namespace General.WPF
{
    static public partial class Extension
    {
        public static bool IsControlDown(this InputEventArgs _)
        {
            return 0 != (ModifierKeys.Control & Keyboard.Modifiers);
        }

        public static bool IsShiftDown(this InputEventArgs _)
        {
            return 0 != (ModifierKeys.Shift & Keyboard.Modifiers);
        }

        public static bool IsAltDown(this InputEventArgs _)
        {
            return 0 != (ModifierKeys.Alt & Keyboard.Modifiers);
        }
    }
}

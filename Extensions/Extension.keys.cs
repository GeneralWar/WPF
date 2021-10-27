using System.Windows.Input;

namespace General.WPF
{
    static public partial class Extension
    {
        public static bool IsControlDown(this InputEventArgs _)
        {
            return 0 != (ModifierKeys.Control & Keyboard.Modifiers);
        }
    }
}

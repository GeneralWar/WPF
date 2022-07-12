using System;
using System.Windows.Input;

static public partial class Extension
{
    public static bool IsControlDown(this EventArgs _)
    {
        return 0 != (ModifierKeys.Control & Keyboard.Modifiers);
    }

    public static bool IsShiftDown(this EventArgs _)
    {
        return 0 != (ModifierKeys.Shift & Keyboard.Modifiers);
    }

    public static bool IsAltDown(this EventArgs _)
    {
        return 0 != (ModifierKeys.Alt & Keyboard.Modifiers);
    }
}

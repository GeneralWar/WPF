using System;
using System.Windows.Input;

static public partial class WPFExtension
{
    public static bool IsModifierKeysDown(this EventArgs _)
    {
        return ModifierKeys.None != Keyboard.Modifiers;
    }

    public static bool IsControlDown(this EventArgs _)
    {
        return ModifierKeys.None != (ModifierKeys.Control & Keyboard.Modifiers);
    }

    public static bool IsShiftDown(this EventArgs _)
    {
        return ModifierKeys.None != (ModifierKeys.Shift & Keyboard.Modifiers);
    }

    public static bool IsAltDown(this EventArgs _)
    {
        return ModifierKeys.None != (ModifierKeys.Alt & Keyboard.Modifiers);
    }
}

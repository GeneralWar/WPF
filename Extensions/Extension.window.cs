using System;
using System.Windows;
using System.Windows.Interop;

static public partial class Extension
{
    static public IntPtr GetWindowHandle(this Window window)
    {
        WindowInteropHelper helper = new WindowInteropHelper(window);
        return window.Dispatcher.Invoke(() => helper.Handle);
    }

    static public bool TrySetDialogResult(this Window window, bool? value)
    {
        try
        {
            window.DialogResult = value;
            return true;
        }
        catch
        {
            return false;
        }
    }
}

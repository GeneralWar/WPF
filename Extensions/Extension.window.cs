using System;
using System.Windows;
using System.Windows.Interop;

static public partial class Extension
{
    static public IntPtr GetWindowHandle(this Window window)
    {
        WindowInteropHelper helper = new WindowInteropHelper(window);
        return helper.Handle;
    }
}

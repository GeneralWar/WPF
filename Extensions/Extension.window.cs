using General.WPF;
using System;
using System.Windows;
using System.Windows.Interop;

static public partial class WPFExtension
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

    static public void ShowWindow<WindowType>(this Window instance, Func<WindowType> creator) where WindowType : Window
    {
        WindowType window = WindowPool.GetOrRegister(creator);
        window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        window.Owner = instance;
        window.Show();
        window.Activate();
    }

    static public bool? ShowDialogWindow<WindowType>(this Window instance, Func<WindowType> creator) where WindowType : Window
    {
        WindowType window = WindowPool.GetOrRegister(creator);
        window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
        window.Owner = instance;
        return window.ShowDialog();
    }
}

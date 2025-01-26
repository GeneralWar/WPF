using General.WPF;
using System;
using System.Windows;
using System.Windows.Input;
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
        try
        {
            WindowType window = WindowPool.GetOrRegister(creator);
            HotKeys.RegisterCloseWindow(window, Key.Escape);
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.Owner = instance;
            window.Show();

            if (WindowState.Minimized == window.WindowState)
            {
                window.WindowState = WindowState.Normal;
            }
            window.Activate();
        }
        catch (Exception e)
        {
            instance.ShowErrorMessageBox(e);
        }
    }

    static public WindowType ShowDialogWindow<WindowType>(this Window instance, Func<WindowType> creator, Action<WindowType>? onClose = null) where WindowType : Window
    {
        return instance.Dispatcher.Invoke(() =>
        {
            WindowType window = WindowPool.GetOrRegister(creator);
            HotKeys.RegisterCloseWindow(window, Key.Escape);
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.Owner = instance;
            if (onClose is not null)
            {
                window.Closed += delegate
                {
                    onClose.Invoke(window);
                };
            }
            window.ShowDialog();
            return window;
        });
    }
}

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

    static public void ShowMessageBox(this Window instance, string message)
    {
        MessageBox.Show(message);
    }

    static public void ShowWarningMessageBox(this Window instance, string message)
    {
        MessageBox.Show(message, "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    static public void ShowErrorMessageBox(this Window instance, string message)
    {
        MessageBox.Show(message, "错误", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}

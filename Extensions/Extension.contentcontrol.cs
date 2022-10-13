using General;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

static public partial class Extension
{
    static public void ShowMessageBox(this ContentControl instance, string message, [CallerFilePath] string? filename = null, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string? memberName = null)
    {
        Tracer.Log(message, filename, lineNumber, memberName);
        instance.Dispatcher.Invoke(() => MessageBox.Show(instance.GetTopWindow(), message));
    }

    static public void ShowWarningMessageBox(this ContentControl instance, string message, [CallerFilePath] string? filename = null, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string? memberName = null)
    {
        Tracer.Warn(message, filename, lineNumber, memberName);
        instance.Dispatcher.Invoke(() => MessageBox.Show(instance.GetTopWindow(), message, "警告", MessageBoxButton.OK, MessageBoxImage.Warning));
    }

    static public void ShowErrorMessageBox(this ContentControl instance, string message, [CallerFilePath] string? filename = null, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string? memberName = null)
    {
        Tracer.Error(message, filename, lineNumber, memberName);
        instance.Dispatcher.Invoke(() => MessageBox.Show(instance.GetTopWindow(), message, "错误", MessageBoxButton.OK, MessageBoxImage.Error));
    }
}

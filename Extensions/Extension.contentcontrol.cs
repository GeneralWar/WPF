using System.Windows;
using System.Windows.Controls;

static public partial class Extension
{
    static public void ShowMessageBox(this ContentControl instance, string message)
    {
        instance.Dispatcher.Invoke(() => MessageBox.Show(instance.GetTopWindow(), message));
    }

    static public void ShowWarningMessageBox(this ContentControl instance, string message)
    {
        instance.Dispatcher.Invoke(() => MessageBox.Show(instance.GetTopWindow(), message, "警告", MessageBoxButton.OK, MessageBoxImage.Warning));
    }

    static public void ShowErrorMessageBox(this ContentControl instance, string message)
    {
        instance.Dispatcher.Invoke(() => MessageBox.Show(instance.GetTopWindow(), message, "错误", MessageBoxButton.OK, MessageBoxImage.Error));
    }
}

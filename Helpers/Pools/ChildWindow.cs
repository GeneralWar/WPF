using System.Windows;

namespace General.WPF
{
    static public class ChildWindow
    {
        static public void Register(Window window)
        {
            window.Closed += delegate
            {
                window.Owner?.Activate();
            };
        }
    }
}

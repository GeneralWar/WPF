using General;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static Extensions;

static public partial class WPFExtension
{
    static public double CheckValidWidth(this FrameworkElement element)
    {
        return element.ActualWidth - element.Margin.Left - element.Margin.Right;
    }

    static public double CheckValidHeight(this FrameworkElement element)
    {
        return element.ActualHeight - element.Margin.Top - element.Margin.Bottom;
    }

    public static Window? GetTopWindow(this FrameworkElement element)
    {
        if (element is Window window)
        {
            return window;
        }
        return Window.GetWindow(element);
    }

    private static FrameworkElement? GetRealParent(this FrameworkElement element)
    {
        if (element.Parent is not null)
        {
            return element.Parent as FrameworkElement;
        }

        object? parent = element.GetPropertyValue("VisualParent");
        if (parent is not null)
        {
            return parent as FrameworkElement;
        }

        if (element.TemplatedParent is not null)
        {
            return element.TemplatedParent as FrameworkElement;
        }

        return null;
    }

    public static bool IsChildOf(this FrameworkElement element, FrameworkElement testParent, bool includeSelf = false)
    {
        if (element == testParent)
        {
            return includeSelf;
        }

        FrameworkElement? parent = element;
        while (parent is not null && (parent = parent.GetRealParent()) != testParent) ;
        return parent == testParent;
    }

    public static bool IsChildOf(this FrameworkElement element, FrameworkElement[] testParents, out FrameworkElement? parent, bool includeSelf = false)
    {
        int index = Array.IndexOf(testParents, element);
        if (includeSelf && index > -1)
        {
            parent = testParents[index];
            return true;
        }

        parent = element;
        while (parent is not null && Array.IndexOf(testParents, parent = parent.GetRealParent()) > -1) ;
        return Array.IndexOf(testParents, parent) > -1;
    }

    static public T? FindAncestor<T>(this IInputElement element) where T : class
    {
        return element.FindAncestor<T>(true);
    }

    static public T? FindAncestor<T>(this IInputElement element, bool includeSelf) where T : class
    {
        FrameworkElement? parent = element as FrameworkElement;
        if (includeSelf && parent is T)
        {
            return parent as T;
        }

        parent = parent?.GetRealParent();
        while (parent is not null && parent is not T && (parent = parent?.GetRealParent()) is not null) ;
        return parent as T;
    }

    static public T? FindAncestor<T>(this IInputElement element, bool includeSelf, Func<T, bool>? filter) where T : class
    {
        FrameworkElement? parent = element as FrameworkElement;
        if (includeSelf && parent is T)
        {
            return parent as T;
        }

        while ((parent = parent?.GetRealParent()) is not null)
        {
            T? t = parent as T;
            if (t is not null && (filter?.Invoke(t) ?? true))
            {
                return t;
            }
        }
        return null;
    }

    static public void RemoveFromParent(this FrameworkElement element)
    {
        element.GetRealParent()?.RemoveChild(element);
    }

    /// <summary>
    /// Delegate to compare source element should be inserted at position of target
    /// </summary>
    /// <param name="target">Element to check if can insert source element at this position</param>
    /// <returns></returns>
    public delegate bool CollectionInsertItemDelegate<T>(T target);

    static public void Insert<T>(this UIElementCollection collection, T item, CollectionInsertItemDelegate<T> compareDelegate) where T : UIElement
    {
        Insert<T>(collection, item, compareDelegate, collection.Insert, collection.Add);
    }

    static public void Insert<T>(this ItemCollection collection, T item, CollectionInsertItemDelegate<T> compareDelegate) where T : UIElement
    {
        Insert<T>(collection, item, compareDelegate, collection.Insert, collection.Add);
    }

    static public void Insert<T>(IEnumerable collection, T item, CollectionInsertItemDelegate<T> compareDelegate, Action<int, T> insert, Func<T, int> add) where T : UIElement
    {
        int index = 0;
        foreach (object o in collection)
        {
            T? record = o as T;
            if (record is null)
            {
                continue;
            }

            if (compareDelegate?.Invoke(record) ?? false)
            {
                insert.Invoke(index, item);
                return;
            }

            ++index;
        }
        add.Invoke(item);
    }

    static public int GetSiblingIndex(this FrameworkElement element)
    {
        ItemsControl? items = element.Parent as ItemsControl;
        if (items is not null)
        {
            return items.Items.IndexOf(element);
        }

        Panel? panel = element.Parent as Panel;
        if (panel is not null)
        {
            return panel.Children.IndexOf(element);
        }

        throw new NotImplementedException();
    }

    static public T? GetFirstChild<T>(this DependencyObject instance) where T : class
    {
        ItemsControl? items = instance as ItemsControl;
        if (items is not null)
        {
            return items.Items.Find<T>(i => i is T);
        }

        Panel? panel = instance as Panel;
        if (panel is not null)
        {
            return panel.Children.Find<T>(i => i is T);
        }

        throw new NotImplementedException();
    }

    static public T? GetLastChild<T>(this DependencyObject instance) where T : class
    {
        ItemsControl? items = instance as ItemsControl;
        if (items is not null)
        {
            return items.Items.OfType<T>().LastOrDefault();
        }

        Panel? panel = instance as Panel;
        if (panel is not null)
        {
            return panel.Children.OfType<T>().LastOrDefault();
        }

        throw new NotImplementedException();
    }

    static public IEnumerator? GetEnumerator(this DependencyObject instance)
    {
        ItemsControl? items = instance as ItemsControl;
        if (items is not null)
        {
            return items.Items.IsEmpty ? null : (items.Items as IEnumerable).GetEnumerator();
        }

        Panel? panel = instance as Panel;
        if (panel is not null)
        {
            return 0 == panel.Children.Count ? null : panel.Children.GetEnumerator();
        }

        throw new NotImplementedException();
    }
    static public void ShowMessageBox(this FrameworkElement instance, string message, [CallerFilePath] string? filename = null, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string? memberName = null)
    {
        Tracer.Log(message, filename, lineNumber, memberName);
        instance.Dispatcher.Invoke(() => MessageBox.Show(instance.GetTopWindow(), message));
    }

    static public void ShowWarningMessageBox(this FrameworkElement instance, string message, [CallerFilePath] string? filename = null, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string? memberName = null)
    {
        Tracer.Warn(message, filename, lineNumber, memberName);
        instance.Dispatcher.Invoke(() => MessageBox.Show(instance.GetTopWindow(), message, "警告", MessageBoxButton.OK, MessageBoxImage.Warning));
    }

    static public void ShowErrorMessageBox(this FrameworkElement instance, string message, [CallerFilePath] string? filename = null, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string? memberName = null)
    {
        Tracer.Error(message, filename, lineNumber, memberName);
        instance.Dispatcher.Invoke(() => MessageBox.Show(instance.GetTopWindow(), message, "错误", MessageBoxButton.OK, MessageBoxImage.Error));
    }

    static public void ShowErrorMessageBox(this FrameworkElement instance, Exception e, [CallerFilePath] string? filename = null, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string? memberName = null)
    {
        Tracer.Exception(e, Assembly.GetCallingAssembly(), filename, lineNumber, memberName);
        instance.Dispatcher.Invoke(() => MessageBox.Show(instance.GetTopWindow(), e.Message, "错误", MessageBoxButton.OK, MessageBoxImage.Error));

        if (e.InnerException is not null)
        {
            ShowErrorMessageBox(instance, e.InnerException, filename, lineNumber, memberName);
        }
    }

    static public MessageBoxResult ShowQuestionMessageBox(this FrameworkElement instance, string message, MessageBoxButton button = MessageBoxButton.YesNo, [CallerFilePath] string? filename = null, [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string? memberName = null)
    {
        Tracer.Log(message, filename, lineNumber, memberName);
        return instance.Dispatcher.Invoke(() => MessageBox.Show(instance.GetTopWindow(), message, "询问", button, MessageBoxImage.Question));
    }

    static public void ExecuteSafely(this FrameworkElement instance, Action action, ExecuteArguments? arguments = null)
    {
        try
        {
            action.Invoke();
        }
        catch (Exception e)
        {
            instance.ShowErrorMessageBox(e);
            arguments?.onException?.Invoke(e);
        }
        finally
        {
            arguments?.onFinally?.Invoke();
        }
    }

    static public async void ExecuteSafely(this FrameworkElement instance, Func<Task> action, ExecuteArguments? arguments = null)
    {
        try
        {
            await Task.Run(action);
        }
        catch (Exception e)
        {
            instance.ShowErrorMessageBox(e);
            arguments?.onException?.Invoke(e);
        }
        finally
        {
            arguments?.onFinally?.Invoke();
        }
    }

    static public void ShowWindow<WindowType>(this FrameworkElement instance, Func<WindowType> creator) where WindowType : Window => instance.GetTopWindow()?.ShowWindow<WindowType>(creator);

    static public WindowType ShowDialogWindow<WindowType>(this FrameworkElement instance, Func<WindowType> creator) where WindowType : Window => instance.GetTopWindow()?.ShowDialogWindow<WindowType>(creator) ?? throw new InvalidOperationException("No top window");
}

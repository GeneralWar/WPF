using System;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

static public partial class Extension
{
    public static Window? GetTopWindow(this FrameworkElement element)
    {
        FrameworkElement? item = element;
        while (null != item && item is not Window)
        {
            item = item.GetRealParent() as FrameworkElement;
        }
        return item as Window;
    }

    private static FrameworkElement? GetRealParent(this FrameworkElement element)
    {
        return element.Parent as FrameworkElement ?? element.TemplatedParent as FrameworkElement;
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

    static public void RemoveFromParent(this FrameworkElement element)
    {
        element?.Parent?.RemoveChild(element);
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
}

using System;
using System.Windows;

namespace General.WPF
{
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

        static public T? FindAncestor<T>(this IInputElement element) where T : FrameworkElement
        {
            return element.FindAncestor<T>(true);
        }

        static public T? FindAncestor<T>(this IInputElement element, bool includeSelf) where T : FrameworkElement
        {
            FrameworkElement? parent = element as FrameworkElement;
            if (includeSelf && parent is T)
            {
                return parent as T;
            }

            parent = parent?.Parent as FrameworkElement;
            while (parent is not null && parent is not T && (parent = parent?.GetRealParent()) is not null) ;
            return parent as T;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace General.WPF
{
    static public partial class Extension
    {
        public static Window GetTopWindow(this FrameworkElement element)
        {
            FrameworkElement item = element;
            while (null != item && item is not Window)
            {
                item = item.Parent as FrameworkElement;
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

        static public T GetElementUpward<T>(this IInputElement element) where T : FrameworkElement
        {
            if (element is T)
            {
                return element as T;
            }

            FrameworkElement parent = element as FrameworkElement;
            while (parent is not T && parent.Parent is not null)
            {
                parent = parent.Parent as FrameworkElement;
            }
            while (parent is not T && parent.TemplatedParent is not null)
            {
                parent = parent.TemplatedParent as FrameworkElement;
            }
            return parent as T;
        }
    }
}

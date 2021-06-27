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
    }
}

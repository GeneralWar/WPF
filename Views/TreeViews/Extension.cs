using System.Windows;

namespace General.WPF
{
    static public partial class Extension
    {
        static public TreeViewItem? GetTreeViewItemUpward(this IInputElement element)
        {
            if (element is TreeViewItem)
            {
                return element as TreeViewItem;
            }

            FrameworkElement? parent = element as FrameworkElement;
            while (parent is not null && (parent = parent.GetRealParent()) is not TreeViewItem) ;
            return parent as TreeViewItem;
        }
    }
}

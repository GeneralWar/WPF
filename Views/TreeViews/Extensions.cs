using System.Windows;

namespace General.WPF
{
    static public partial class Extensions
    {
        static public TreeViewItem GetTreeViewItemUpward(this IInputElement element)
        {
            if (element is TreeViewItem)
            {
                return element as TreeViewItem;
            }

            FrameworkElement parent = element as FrameworkElement;
            while (parent is not TreeViewItem && parent.Parent is not null)
            {
                parent = parent.Parent as FrameworkElement;
            }
            while (parent is not TreeViewItem && parent.TemplatedParent is not null)
            {
                parent = parent.TemplatedParent as FrameworkElement;
            }
            return parent as TreeViewItem;
        }
    }
}

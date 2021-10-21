using System.Windows;

namespace General.WPF
{
    static public partial class Extension
    {
        public static TreeView? GetTreeViewOwner(this TreeViewItem item)
        {
            if (item.Parent is TreeView)
            {
                return item.Parent as TreeView;
            }
            return (item.Parent as TreeViewItem)?.GetTreeViewOwner();
        }
    }
}

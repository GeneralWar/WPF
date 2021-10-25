namespace General.WPF
{
    static public partial class Extension
    {
        public static TreeView? GetTreeViewOwner(this TreeViewItem item)
        {
            return item.FindAncestor<TreeView>();
        }
    }
}

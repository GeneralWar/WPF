using General.WPF;
using System;

static public partial class Extension
{
    public static TreeView? GetTreeViewOwner(this TreeViewItem item)
    {
        return item.FindAncestor<TreeView>();
    }

    public static int GetSiblingIndex(this TreeViewItem item)
    {
        TreeView? tree = item.Parent as TreeView;
        if (tree is not null)
        {
            return tree.Items.IndexOf(item);
        }

        TreeViewItem? parent = item.Parent as TreeViewItem;
        if (parent is not null)
        {
            return parent.Items.IndexOf(item);
        }

        throw new Exception("Unexpected condition");
    }
}

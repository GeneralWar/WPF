using General.WPF;
using System;
using System.Linq;

static public partial class Extension
{
    static public void ExpandAll(this System.Windows.Controls.TreeView instance)
    {
        foreach (System.Windows.Controls.TreeViewItem item in instance.Items.OfType<System.Windows.Controls.TreeViewItem>())
        {
            item.ExpandSubtree();
        }
    }

    static public void CollapseAll(this System.Windows.Controls.TreeView instance)
    {
        foreach (System.Windows.Controls.TreeViewItem item in instance.Items.OfType<System.Windows.Controls.TreeViewItem>())
        {
            item.CollapseSubtree();
        }
    }

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

    public static void CollapseSubtree(this System.Windows.Controls.TreeViewItem item)
    {
        foreach (System.Windows.Controls.TreeViewItem i in item.Items.OfType<System.Windows.Controls.TreeViewItem>())
        {
            i.CollapseSubtree();
        }
        item.IsExpanded = false;
    }
}

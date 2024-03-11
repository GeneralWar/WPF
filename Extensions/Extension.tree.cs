using General.WPF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

static public partial class WPFExtension
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

    static public TreeView? GetTreeViewOwner(this TreeViewItem item)
    {
        return item.FindAncestor<TreeView>();
    }

    static public int GetSiblingIndex(this TreeViewItem item)
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

    static public void CollapseSubtree(this System.Windows.Controls.TreeViewItem item)
    {
        foreach (System.Windows.Controls.TreeViewItem i in item.Items.OfType<System.Windows.Controls.TreeViewItem>())
        {
            i.CollapseSubtree();
        }
        item.IsExpanded = false;
    }

    static public System.Windows.Controls.TreeViewItem[] GetPath(this System.Windows.Controls.TreeViewItem item)
    {
        if (item is null)
        {
            return new TreeViewItem[0];
        }

        System.Windows.Controls.TreeViewItem? current = item;
        List<System.Windows.Controls.TreeViewItem> path = new List<System.Windows.Controls.TreeViewItem>();
        do
        {
            path.Insert(0, current);
            current = current.Parent as System.Windows.Controls.TreeViewItem;
        } while (null != current);
        return path.ToArray();
    }

    static public string GetPathString(this System.Windows.Controls.TreeViewItem item) => string.Join(Path.DirectorySeparatorChar, GetPath(item).Select(i => i.Header));
}

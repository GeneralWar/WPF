using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace General.WPF
{
    static public partial class Extension
    {
        public static TreeView GetTreeViewOwner(this TreeViewItem item)
        {
            if (item.Parent is TreeView)
            {
                return item.Parent as TreeView;
            }
            return (item.Parent as TreeViewItem)?.GetTreeViewOwner();
        }
    }
}

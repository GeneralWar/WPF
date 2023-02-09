using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;
using TreeViewItem = General.WPF.TreeViewItem;

namespace Genera.WPF.Test
{
    /// <summary>
    /// TreeViewTestWindow.xaml 的交互逻辑
    /// </summary>
    public partial class TreeViewTestWindow : Window
    {
        private ContextMenu mContextMenu;

        public TreeViewTestWindow()
        {
            InitializeComponent();

            mContextMenu = this.createContextMenu();
            mTreeView.ExpandAll();
            foreach (TreeViewItem item in mTreeView.Items.OfType<TreeViewItem>())
            {
                item.ContextMenu = mContextMenu;
            }
        }

        private ContextMenu createContextMenu()
        {
            ContextMenu menu = new ContextMenu();

            MenuItem create = new MenuItem();
            create.Header = "Create";
            create.Click += this.onCreateClick;
            menu.Items.Add(create);

            return menu;
        }

        private void onCreateClick(object sender, RoutedEventArgs e)
        {
            MenuItem? item = sender as MenuItem;
            if (item is null)
            { return;
            }

            ContextMenu? menu = item.FindAncestor<ContextMenu>();
            if (menu is null)
            {
                return;
            }

            TreeViewItem? parentItem = menu.PlacementTarget as TreeViewItem;
            if (parentItem is not null)
            {
                parentItem.IsExpanded = true;
            }

            ItemsControl? parent = parentItem as ItemsControl ?? mTreeView;
            if (parent is null)
            {
                return;
            }

            TreeViewItem treeItem = new TreeViewItem();
            treeItem.OnEditCancel += i => i.RemoveFromParent();
            treeItem.Header = "New TreeViewItem";
            parent.Items.Add(treeItem);
            treeItem.Edit();
        }

        private void onItemMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem? item = sender as TreeViewItem;
            if (item is null)
            {
                return;
            }

            item.IsExpanded = !item.IsExpanded;
        }
    }
}

using General.WPF;
using System;
using System.Windows;
using static General.WPF.TreeView;

namespace Samples
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            mTreeView.onDrop += onTreeViewDrop;
        }

        private void onTreeViewDrop(General.WPF.TreeView.DragEvent e)
        {
            TreeViewItem? sourceItem = e.SourceItem as TreeViewItem;
            ITreeViewItemCollection targetItem = e.TargetItem;
            if (sourceItem is null)
            {
                return;
            }

            sourceItem.RemoveFromParent();

            if (DragModes.Drop == e.Mode)
            {
                targetItem.Items.Add(sourceItem);
            }
            else if (DragModes.InsertFront == e.Mode)
            {
                targetItem.Parent?.Items.Insert(targetItem.SiblingIndex, sourceItem);
            }
            else if (DragModes.InsertBack == e.Mode)
            {
                targetItem.Parent?.Items.Insert(targetItem.SiblingIndex + 1, sourceItem);
            }
        }
    }
}

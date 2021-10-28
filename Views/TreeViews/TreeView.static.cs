using System;
using System.Reflection;
using System.Windows;

namespace General.WPF
{
    /// <summary>
    /// TreeView.xaml 的交互逻辑
    /// </summary>
    public partial class TreeView : System.Windows.Controls.TreeView, ITreeViewItemCollection, IMultipleSelectionsCollection
    {
        public enum DragModes
        {
            /// <summary>
            /// Drop source into target item
            /// </summary>
            Drop = 1,
            /// <summary>
            /// Insert source before or after target item
            /// </summary>
            Insert = 2,

            All = Drop | Insert,
        }

        private const double DRAG_EFFECT_RATE = .1;

        static private readonly PropertyInfo? IsSelectionChangeActiveProperty = typeof(TreeView).GetProperty("IsSelectionChangeActive", BindingFlags.NonPublic | BindingFlags.Instance);
        static private readonly DependencyPropertyKey SelectedItemPropertyKey;

        static public DependencyProperty AllowItemDragProperty = DependencyProperty.Register(nameof(AllowItemDrag), typeof(bool), typeof(TreeView), new PropertyMetadata(true));
        static public DependencyProperty ItemDragModeProperty = DependencyProperty.Register(nameof(ItemDragMode), typeof(DragModes), typeof(TreeView), new PropertyMetadata(DragModes.All));

        static TreeView()
        {
            DependencyPropertyKey? key = typeof(System.Windows.Controls.TreeView).GetField("SelectedItemPropertyKey", BindingFlags.Static | BindingFlags.NonPublic)?.GetValue(null) as DependencyPropertyKey;
            if (key is null)
            {
                throw new NullReferenceException();
            }
            SelectedItemPropertyKey = key;
        }
    }
}

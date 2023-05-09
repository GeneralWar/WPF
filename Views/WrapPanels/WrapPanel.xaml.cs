using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace General.WPF
{
    /// <summary>
    /// WrapPanel.xaml 的交互逻辑
    /// </summary>
    public partial class WrapPanel : System.Windows.Controls.UserControl, IMultipleSelectionsCollection
    {
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(WrapPanel));
        public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register(nameof(ItemHeight), typeof(double), typeof(WrapPanel));

        public Orientation Orientation { get => (Orientation)this.GetValue(OrientationProperty); set => this.SetValue(OrientationProperty, value); }
        public double ItemHeight { get => (double)this.GetValue(ItemHeightProperty); set => this.SetValue(ItemHeightProperty, value); }

        public UIElementCollection Children => mCore.Children;
        IEnumerable<IMultipleSelectionsItem> IMultipleSelectionsCollection.Items => mCore.Children.OfType<IMultipleSelectionsItem>();

        private List<IMultipleSelectionsItem> mSelectedItems = new List<IMultipleSelectionsItem>();
        public IEnumerable<IMultipleSelectionsItem> SelectedItems => mSelectedItems.ToArray();

        public IMultipleSelectionsItem? LastSelected { get; private set; }

        public WrapPanel()
        {
            InitializeComponent();
        }

        void IMultipleSelectionsCollection.Select(IMultipleSelectionsItem item)
        {
            foreach (IMultipleSelectionsItem record in mSelectedItems)
            {
                if (record == item)
                {
                    continue;
                }

                record.IsSelected = false;
            }

            mSelectedItems.Clear();
            (this as IMultipleSelectionsCollection).Append(item);
        }

        void IMultipleSelectionsCollection.Append(IMultipleSelectionsItem item)
        {
            mSelectedItems.Add(item);
            item.IsSelected = true;
            this.LastSelected = item;
        }

        void IMultipleSelectionsCollection.Unselect(IMultipleSelectionsItem item)
        {
            mSelectedItems.Remove(item);
            item.IsSelected = false;

            if (this.LastSelected == item)
            {
                this.LastSelected = null;
            }
        }

        private void ClearAllSelections()
        {
            foreach (IMultipleSelectionsItem record in mSelectedItems)
            {
                record.IsSelected = false;
            }
            mSelectedItems.Clear();
        }

        void IMultipleSelectionsCollection.ClearAllSelections()
        {
            this.ClearAllSelections();
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            IInputElement input = this.InputHitTest(e.GetPosition(this));
            IMultipleSelectionsItem? item = input.FindAncestor<IMultipleSelectionsItem>();
            if (item is null)
            {
                this.ClearAllSelections();
            }
            else
            {
                (this as IMultipleSelectionsCollection).Select(item);
            }
        }

        void IMultipleSelectionsCollection.SelectTo(IMultipleSelectionsItem item)
        {
            if (mSelectedItems.Contains(item))
            {
                return;
            }

            IMultipleSelectionsItem? selectedItem = this.LastSelected;
            if (selectedItem is null)
            {
                (this as IMultipleSelectionsCollection).Append(item);
                return;
            }

            IMultipleSelectionsItem from = selectedItem;
            IMultipleSelectionsItem to = item;
            if (selectedItem.SiblingIndex > item.SiblingIndex)
            {
                from = item;
                to = selectedItem;
            }

            IEnumerator enumerator = this.Children.GetEnumerator();
            while (enumerator.MoveNext() && enumerator.Current != from) ;
            (this as IMultipleSelectionsCollection).Append(from);

            while (enumerator.MoveNext() && enumerator.Current != to)
            {
                IMultipleSelectionsItem? i = enumerator.Current as IMultipleSelectionsItem;
                if (i is not null)
                {
                    (this as IMultipleSelectionsCollection).Append(i);
                }
            }
            (this as IMultipleSelectionsCollection).Append(to);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
        }
    }
}

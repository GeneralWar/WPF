using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace General.WPF
{
    /// <summary>
    /// WrapPanel.xaml 的交互逻辑
    /// </summary>
    public partial class WrapPanel : System.Windows.Controls.WrapPanel, IMultipleSelectionsCollection
    {
        private List<IMultipleSelectionsItem> mSelectedItems = new List<IMultipleSelectionsItem>();
        public IEnumerable<IMultipleSelectionsItem> SelectedItems => mSelectedItems.ToArray();

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
            mSelectedItems.Add(item);
            item.IsSelected = true;
        }

        void IMultipleSelectionsCollection.Append(IMultipleSelectionsItem item)
        {
            mSelectedItems.Add(item);
            item.IsSelected = true;
        }

        void IMultipleSelectionsCollection.Unselect(IMultipleSelectionsItem item)
        {
            mSelectedItems.Remove(item);
            item.IsSelected = false;
        }

        private void ClearAllSelections()
        {
            foreach (IMultipleSelectionsItem record in mSelectedItems)
            {
                record.IsSelected = false;
            }
            mSelectedItems.Clear();
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
    }
}

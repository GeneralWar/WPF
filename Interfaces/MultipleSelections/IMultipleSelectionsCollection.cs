using System.Collections.Generic;

namespace General.WPF
{
    public interface IMultipleSelectionsCollection
    {
        void Select(IMultipleSelectionsItem item);
        void SelectTo(IMultipleSelectionsItem item);

        void Append(IMultipleSelectionsItem item);

        void Unselect(IMultipleSelectionsItem item);

        void ClearAllSelections();

        IEnumerable<IMultipleSelectionsItem> SelectedItems { get; }
    }

    public interface IMultipleSelectionsItem
    {
        IMultipleSelectionsCollection Collection { get; }
        bool IsSelected { get; set; }
    }
}

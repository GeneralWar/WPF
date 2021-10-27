using System.Collections.Generic;

namespace General.WPF
{
    public interface IMultipleSelectionsCollection
    {
        void Select(IMultipleSelectionsItem item);
        void Append(IMultipleSelectionsItem item);
        void Unselect(IMultipleSelectionsItem item);

        IEnumerable<IMultipleSelectionsItem> SelectedItems { get; }
    }

    public interface IMultipleSelectionsItem
    {
        IMultipleSelectionsCollection Collection { get; }
        bool IsSelected { get; set; }
    }
}

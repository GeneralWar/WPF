using System.Collections.Generic;
using System.Windows;

namespace General.WPF
{
    public interface IMultipleSelectionsCollection
    {
        IMultipleSelectionsItem? LastSelected { get; }

        void Select(IMultipleSelectionsItem item);
        void SelectTo(IMultipleSelectionsItem item);

        void Append(IMultipleSelectionsItem item);

        void Unselect(IMultipleSelectionsItem item);

        void ClearAllSelections();

        IEnumerable<IMultipleSelectionsItem> Items { get; }
        IEnumerable<IMultipleSelectionsItem> SelectedItems { get; }
    }

    public interface IMultipleSelectionsItem
    {
        DependencyObject Parent { get; }

        IMultipleSelectionsCollection? Collection { get; }
        bool IsSelected { get; set; }
        int SiblingIndex { get; }
    }
}

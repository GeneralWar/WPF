using System.Collections.Generic;
using System.Linq;

namespace General.WPF
{
    static public partial class Extension
    {
        public static bool IsOnlySelected(this IMultipleSelectionsCollection collection, IMultipleSelectionsItem item)
        {
            IEnumerable<IMultipleSelectionsItem> selections = collection.SelectedItems;
            return 1 == selections.Count() && item == selections.First();
        }

        public static bool IsOnlySelected(this IMultipleSelectionsItem item)
        {
            IEnumerable<IMultipleSelectionsItem>? selections = item.Collection?.SelectedItems;
            return selections is not null && 1 == selections.Count() && item == selections.First();
        }
    }
}

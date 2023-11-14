using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;

static public partial class Extension
{
    static public void ApplySorting(this DataGrid instance)
    {
        SortDescription[] descriptions = instance.Items.SortDescriptions.ToArray();
        instance.Items.SortDescriptions.Clear();
        foreach (SortDescription description in descriptions)
        {
            instance.Items.SortDescriptions.Add(description);
        }
    }
}

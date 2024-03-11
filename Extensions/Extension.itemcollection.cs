using System.Collections;
using System.Windows.Controls;

static public partial class WPFExtension
{
    static public void AddRange(this ItemCollection instance, IEnumerable items)
    {
        foreach (object item in items)
        {
            instance.Add(item);
        }
    }
}

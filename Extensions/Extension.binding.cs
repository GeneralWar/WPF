using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Data;

static public partial class Extension
{
    static public void RemoveBindings(this FrameworkElement instance, bool recursively = false)
    {
        foreach (DependencyProperty property in instance.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy).Where(f => f.FieldType == typeof(DependencyProperty)).Select(f => f.GetValue(null)).OfType<DependencyProperty>())
        {
            BindingExpression? expression = instance.GetBindingExpression(property);
            if (expression is not null)
            {
                BindingOperations.ClearBinding(instance, property);
            }
        }

        if (recursively)
        {
            EnumerateChildren<FrameworkElement>(instance, c => RemoveBindings(c, recursively));
        }
    }
}

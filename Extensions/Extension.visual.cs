using System.Reflection;
using System.Windows;
using System.Windows.Media;

static public partial class Extension
{
    public static Vector GetVisualOffset(this Visual visual)
    {
        PropertyInfo? info = visual.GetType().GetProperty("VisualOffset", BindingFlags.NonPublic | BindingFlags.Instance);
        return (Vector)(info?.GetValue(visual) ?? default(Vector));
    }
}

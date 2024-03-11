using System.Reflection;
using System.Windows;
using System.Windows.Media;

static public partial class WPFExtension
{
    static private readonly LengthConverter LengthConverter = new LengthConverter();
    static private readonly ThicknessConverter ThicknessConverter = new ThicknessConverter();

    public static Vector GetVisualOffset(this Visual visual)
    {
        PropertyInfo? info = visual.GetType().GetProperty("VisualOffset", BindingFlags.NonPublic | BindingFlags.Instance);
        return (Vector)(info?.GetValue(visual) ?? default(Vector));
    }

    static public Thickness ConvertToThickness(this Visual _, string value)
    {
        return (Thickness)(ThicknessConverter.ConvertFromString(value.Trim()) ?? new Thickness());
    }
}

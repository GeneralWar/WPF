using System.Reflection;
using System.Windows;
using System.Windows.Controls;
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
    
    static public double CheckValidWidth(this Control element)
    {
        return element.ActualWidth - element.Margin.Left - element.Margin.Right - element.Padding.Left - element.Padding.Right;
    }

    static public double CheckValidHeight(this Control element)
    {
        return element.ActualHeight - element.Margin.Top - element.Margin.Bottom - element.Padding.Top - element.Padding.Bottom;
    }
}

using System.Linq;
using System.Windows;
using System.Windows.Controls;

static public partial class Extension
{
    static public UIElement? GetItem(this Grid instance, int rowIndex, int columnIndex)
    {
        return instance.Children.OfType<UIElement>().FirstOrDefault(e => Grid.GetRow(e) == rowIndex && Grid.GetColumn(e) == columnIndex);
    }
}

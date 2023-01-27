using System.Windows;

static public partial class Extension
{
    private const string DRAG_SOURCE = "DragSource";

    static public DataObject ToDragData(this FrameworkElement element)
    {
        System.Windows.DataObject data = new DataObject();
        data.SetData(DRAG_SOURCE, element);
        return data;
    }

    static public object? GetDragSource(this DragEventArgs e)
    {
        return e.Data.GetData(DRAG_SOURCE);
    }
}

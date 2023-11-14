using System;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

static public partial class Extension
{
    static public void AddItem(this MenuBase instance, string header, Action<MenuItem> handler)
    {
        MenuItem item = new MenuItem();
        item.Header = header;
        item.Click += (s, e) => handler.Invoke(item);
        instance.Items.Add(item);
    }
}

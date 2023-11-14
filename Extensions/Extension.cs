using System;
using System.Windows;
using System.Windows.Controls;

static public partial class Extension
{
    static public void EnumerateChildren(this DependencyObject instance, Action<DependencyObject> action)
    {
        EnumerateChildren<DependencyObject>(instance, action);
    }

    static private void invoke_enumerate_action<ChildType>(Action<ChildType> action, ChildType? child) where ChildType : DependencyObject
    {
        if (child is not null)
        {
            action.Invoke(child);
        }
    }

    static public void EnumerateChildren<ChildType>(this DependencyObject instance, Action<ChildType> action) where ChildType : DependencyObject
    {
        ContentControl? content = instance as ContentControl;
        if (content is not null)
        {
            invoke_enumerate_action<ChildType>(action, content.Content as ChildType);
            return;
        }

        ItemsControl? items = instance as ItemsControl;
        if (items is not null)
        {
            foreach (object item in items.Items)
            {
                invoke_enumerate_action<ChildType>(action, item as ChildType);
            }
            return;
        }

        Panel? panel = instance as Panel;
        if (panel is not null)
        {
            foreach (object child in panel.Children)
            {
                invoke_enumerate_action<ChildType>(action, child as ChildType);
            }
            return;
        }
    }
}

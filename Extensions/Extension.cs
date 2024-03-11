using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Windows;
using System.Windows.Controls;

static public partial class WPFExtension
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
    static public List<T> GetAllChildren<T>(this ContentControl control, bool recursively) where T : Control
    {
        List<T> items = new List<T>();
        object content = control.Content;
        Panel? panel = content as Panel;
        if (panel is not null)
        {
            items.AddRange(panel.GetAllChildren<T>(recursively));
        }
        else
        {
        }
        return items;
    }

    static public List<T> GetAllChildren<T>(this Panel control, bool recursively) where T : Control
    {
        List<T> items = new List<T>();
        foreach (var child in control.Children)
        {
            T? t = child as T;
            if (t is not null)
            {
                items.Add(t);
            }

            Panel? panel = child as Panel;
            if (recursively && panel is not null)
            {
                items.AddRange(GetAllChildren<T>(panel, recursively));
            }
        }
        return items;
    }

    //static public List<T> GetAllChildren<T>(this Panel control, bool recursively) where T : Control
    //{
    //    List<T> items = new List<T>();
    //    int count = VisualTreeHelper.GetChildrenCount(control);
    //    for (int i = 0; i < count; ++i)
    //    {
    //        var item = VisualTreeHelper.GetChild(control, i);
    //        if (item is T)
    //        {
    //            items.Add(item as T);
    //            if (recursively)
    //            {
    //                items.AddRange(item.GetAllChildren<T>(recursively));
    //            }
    //        }
    //    }
    //    return items;
    //}

    static public T? Find<T>(this ItemCollection collection, Func<T, bool> predicate)
    {
        int count = collection.Count;
        for (int i = 0; i < count; ++i)
        {
            object o = collection[i];
            if (o is not T)
            {
                continue;
            }

            T item = (T)o;
            if (predicate?.Invoke(item) ?? false)
            {
                return item;
            }
        }
        return default(T);
    }

    static public bool CheckReverse(this TextChange source, TextChange target)
    {
        return source.Offset == target.Offset && source.AddedLength == target.RemovedLength && source.RemovedLength == target.AddedLength;
    }

    static public void Merge(this TextChange source, TextChange target)
    {
        if (source.Offset + source.AddedLength - source.RemovedLength != target.Offset)
        {
            throw new InvalidOperationException($"Try to merge two {nameof(TextChange)}s, but information mismatch.");
        }

        source.SetPropertyValue(nameof(TextChange.AddedLength), source.AddedLength + target.AddedLength);
        source.SetPropertyValue(nameof(TextChange.RemovedLength), source.RemovedLength + target.RemovedLength);
    }

    static public void Merge(this TextChange[] sources, IEnumerable<TextChange> targets)
    {
        if (sources.Length != targets.Count())
        {
            throw new InvalidOperationException($"Try to merge {nameof(TextChange)} collection, but collection's count mismatch.");
        }

        for (int i = 0; i < sources.Length; ++i)
        {
            sources[i].Merge(targets.ElementAt(i));
        }
    }
}

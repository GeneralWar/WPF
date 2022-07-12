using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

static public partial class Extension
{
    static public void RemoveChild(this DependencyObject parent, UIElement child)
    {
        Panel? panel = parent as Panel;
        if (panel is not null)
        {
            panel.Children.Remove(child);
            return;
        }

        Decorator? decorator = parent as Decorator;
        if (decorator is not null)
        {
            if (decorator.Child == child)
            {
                decorator.Child = null;
            }
            return;
        }

        ContentPresenter? contentPresenter = parent as ContentPresenter;
        if (contentPresenter is not null)
        {
            if (contentPresenter.Content == child)
            {
                contentPresenter.Content = null;
            }
            return;
        }

        ContentControl? contentControl = parent as ContentControl;
        if (contentControl is not null)
        {
            if (contentControl.Content == child)
            {
                contentControl.Content = null;
            }
            return;
        }

        ItemsControl? itemsControl = parent as ItemsControl;
        if (itemsControl is not null)
        {
            itemsControl.Items.Remove(child);
            return;
        }

        // maybe more
        Trace.Assert(false);
    }

    static public T? FindDataContextUpward<T>(this DependencyObject instance) where T : class
    {
        FrameworkElement? frameworkElement = instance as FrameworkElement;
        if (frameworkElement is not null)
        {
            if (frameworkElement.DataContext is T)
            {
                return frameworkElement.DataContext as T;
            }

            return frameworkElement.Parent?.FindDataContextUpward<T>();
        }

        FrameworkContentElement? frameworkContentElement = instance as FrameworkContentElement;
        if (frameworkContentElement is not null)
        {
            if (frameworkContentElement.DataContext is T)
            {
                return frameworkContentElement.DataContext as T;
            }

            return frameworkContentElement.Parent?.FindDataContextUpward<T>();
        }

        throw new NotImplementedException();
    }
}

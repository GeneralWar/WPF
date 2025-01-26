using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

static public partial class WPFExtension
{
    static private void record_items_enable_status(DispatcherObject instance, Dictionary<object, bool> map, IEnumerable<UIElement> disabledItems)
    {
        instance.Dispatcher.InvokeAsync(() =>
        {
            foreach (UIElement i in disabledItems)
            {
                map[i] = i.IsEnabled;
                i.IsEnabled = false;
            }
        });
    }

    static private void restore_items_enable_status(DispatcherObject instance, Dictionary<object, bool> map, IEnumerable<UIElement> disabledItems)
    {
        instance.Dispatcher.InvokeAsync(() =>
        {
            foreach (UIElement i in disabledItems)
            {
                i.IsEnabled = map[i];
            }
        });
    }

    [Obsolete]
    static public async Task ExecuteAsync(this DispatcherObject instance, Func<Task> action, params UIElement[] disableItems)
    {
        Dictionary<object, bool> enableMap = new Dictionary<object, bool>();
        IEnumerable<UIElement> disables = disableItems.Distinct();
        record_items_enable_status(instance, enableMap, disables);
        await action.Invoke();
        restore_items_enable_status(instance, enableMap, disables);
    }

    [Obsolete]
    static public async Task<T> ExecuteAsync<T>(this DispatcherObject instance, Func<Task<T>> action, params UIElement[] disabledItems)
    {
        Dictionary<object, bool> enableMap = new Dictionary<object, bool>();
        IEnumerable<UIElement> disables = disabledItems.Distinct();
        record_items_enable_status(instance, enableMap, disables);
        T result = await action.Invoke();
        restore_items_enable_status(instance, enableMap, disables);
        return result;
    }
}

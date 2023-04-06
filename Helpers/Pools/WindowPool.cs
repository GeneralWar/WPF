using System;
using System.Collections.Generic;
using System.Windows;

namespace General.WPF
{
    static public class WindowPool
    {
        static private Dictionary<Type, Window> sWindows = new Dictionary<Type, Window>();

        static public void Register(Window window)
        {
            Type type = window.GetType();
            if (!sWindows.TryAdd(type, window))
            {
                if (sWindows[type] == window)
                {
                    return;
                }

                throw new InvalidOperationException($"{nameof(WindowPool)}: type {type} already exists");
            }

            window.Closing += delegate { sWindows.Remove(window.GetType()); };
        }

        static public T Register<T>() where T : Window, new()
        {
            return Application.Current.Dispatcher.Invoke(() =>
            {
                T window = new T();
                Register(window);
                return window;
            });
        }

        static public T Register<T>(Func<T> creator) where T : Window
        {
            return Application.Current.Dispatcher.Invoke(() =>
            {
                T window = creator.Invoke();
                Register(window);
                return window;
            });
        }

        static public T? Get<T>() where T : Window
        {
            Window? window;
            sWindows.TryGetValue(typeof(T), out window);
            return window as T;
        }

        static public T GetOrRegister<T>() where T : Window, new()
        {
            T? window = Get<T>();
            if (window is null)
            {
                window = Register<T>();
            }
            return window;
        }

        static public T GetOrRegister<T>(Func<T> creator) where T : Window
        {
            T? window = Get<T>();
            if (window is null)
            {
                window = Register<T>(creator);
            }
            return window;
        }
    }
}

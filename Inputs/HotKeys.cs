using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace General.WPF
{
    static public class HotKeys
    {
        static private Dictionary<ulong, List<Action>> sKeyMap = new Dictionary<ulong, List<Action>>();

        static private ulong MakeKey(Key key, ModifierKeys modifiers)
        {
            return (((ulong)key) << 32) + (ulong)modifiers;
        }

        static public void Register(Key key, ModifierKeys modifiers, Action action)
        {
            List<Action> actions;
            ulong realKey = MakeKey(key, modifiers);
            if (!sKeyMap.TryGetValue(realKey, out actions))
            {
                sKeyMap.Add(realKey, actions = new List<Action>());
            }
            actions.Add(action);
        }

        static public void Unregister(Key key, ModifierKeys modifiers, Action action)
        {
            List<Action> actions;
            ulong realKey = MakeKey(key, modifiers);
            if (!sKeyMap.TryGetValue(realKey, out actions))
            {
                return;
            }

            int index = actions.FindIndex(a => a.Target == action.Target && a.Method == action.Method);
            if (index > -1)
            {
                actions.RemoveAt(index);
            }
        }

        static public void Update(KeyEventArgs e)
        {
            List<Action> actions;
            ulong key = MakeKey(e.Key, Keyboard.Modifiers);
            if (!sKeyMap.TryGetValue(key, out actions))
            {
                return;
            }

            foreach (Action action in actions.ToArray())
            {
                action?.Invoke();
            }
        }
    }
}

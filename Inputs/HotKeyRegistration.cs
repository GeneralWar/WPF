using System;
using System.Windows;
using System.Windows.Input;

namespace General.WPF
{
    internal class HotKeyRegistration
    {
        public Key Key { get; }
        private Action mAction;

        private Window? mWindow;

        internal HotKeyRegistration(Key key, Action action)
        {
            this.Key = key;
            mAction = action;
        }

        public void Match(KeyEventArgs e)
        {
            if (this.Key != e.Key || e.IsModifierKeysDown())
            {
                return;
            }

            mAction.Invoke();
        }

        public void BindWindow(Window window)
        {
            mWindow = window;
            window.KeyDown += this.onKeyDown;
            window.Closed += this.onClosed;
        }

        private void onKeyDown(object sender, KeyEventArgs e)
        {
            this.Match(e);
        }

        private void onClosed(object? sender, EventArgs e)
        {
            if (mWindow is null)
            {
                return;
            }

            mWindow.KeyDown -= this.onKeyDown;
            mWindow.Closed -= this.onClosed;
        }
    }
}

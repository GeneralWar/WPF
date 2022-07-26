using General.Jsons;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace General.WPF
{
    /// <summary>
    /// Helper to cache window location on close, and will restore location when window of the same type load
    /// </summary>
    public class WindowLocationCache : HelperCache
    {
        public override string CachePath => Path.Join(base.CachePath, this.Window.GetType().Name + ".json");

        public Window Window { get; private set; }

        [DataField] public int LocationX { get; private set; }
        [DataField] public int LocationY { get; private set; }
        [DataField] public int Width { get; private set; }
        [DataField] public int Height { get; private set; }

        private WindowLocationCache(Window window)
        {
            this.Window = window;
            window.Initialized += this.onInitialized;
            window.Closing += this.onClosing;
        }

        private void onInitialized(object? sender, EventArgs e)
        {
            byte[]? data = this.readFromCache();
            if (data is null)
            {
                return;
            }

            Json.Parse(Encoding.UTF8.GetString(data))?.Deserialize(this);
            this.Window.Left = this.LocationX;
            this.Window.Top = this.LocationY;
            this.Window.Width = this.Width;
            this.Window.Height = this.Height;
        }

        private void onClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.LocationX = (int)this.Window.Left;
            this.LocationY = (int)this.Window.Top;
            this.Width = (int)this.Window.ActualWidth;
            this.Height = (int)this.Window.ActualHeight;
            this.writeToCache(Encoding.UTF8.GetBytes(Json.ToJson(this)));
        }

        static public void Register(Window window)
        {
            new WindowLocationCache(window);
        }
    }
}

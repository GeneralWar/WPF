using General.Jsons;
using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Forms;

namespace General.WPF
{
    /// <summary>
    /// Helper to cache window location on close, and will restore location when window of the same type load
    /// </summary>
    public class WindowLocationCache : HelperCache
    {
        private string? mRelativePath = null;
        public override string CachePath => string.IsNullOrWhiteSpace(mRelativePath) ? Path.Join(base.CachePath, this.Window.GetType().Name + ".json") : Path.Join(base.CachePath, mRelativePath, this.Window.GetType().Name + ".json");

        [NonDataField] public Window Window { get; private set; }

        [DataField] public int LocationX { get; private set; }
        [DataField] public int LocationY { get; private set; }
        [DataField] public int Width { get; private set; }
        [DataField] public int Height { get; private set; }

        private WindowLocationCache(Window window) : this(window, null) { }

        private WindowLocationCache(Window window, string? relativePath)
        {
            mRelativePath = relativePath;

            this.Window = window;
            window.Closing += this.onClosing;
            if (this.restore())
            {
                window.Initialized += this.onInitialized;
                window.Activated += this.onShow;
            }
        }

        private bool restore()
        {
            byte[]? data = this.readFromCache();
            if (data is null)
            {
                return false;
            }

            string content = Encoding.UTF8.GetString(data);
            Json.Parse(content)?.Deserialize(this);
            return true;
        }

        private void onInitialized(object? sender, EventArgs e)
        {
            int totalWidth = 0, totalHeight = 0;
            Screen[] screens = Screen.AllScreens;
            foreach (Screen screen in screens)
            {
                totalWidth += screen.Bounds.Width;
                totalHeight += screen.Bounds.Height;
            }

            if (this.LocationX > -this.Width && this.LocationX < totalWidth)
            {
                this.Window.Left = this.LocationX;
            }
            if (this.LocationY >= 0 && this.LocationY < totalHeight)
            {
                this.Window.Top = this.LocationY;
            }
            if (this.Width > 0)
            {
                this.Window.Width = this.Width;
            }
            if (this.Height > 0)
            {
                this.Window.Height = this.Height;
            }
        }

        private void onShow(object? sender, EventArgs e)
        {
            this.Window.Activated -= this.onShow;
            this.onInitialized(sender, e);
        }

        private void onClosing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            this.LocationX = (int)this.Window.Left;
            this.LocationY = (int)this.Window.Top;
            this.Width = (int)this.Window.ActualWidth;
            this.Height = (int)this.Window.ActualHeight;
            this.writeToCache(Encoding.UTF8.GetBytes(Json.ToJson(this)));
        }

        /// <summary>
        /// Will load cache (if exists) and save cache to the predetermined path.
        /// </summary>
        static public void Register(Window window)
        {
            new WindowLocationCache(window);
        }

        /// <summary>
        /// Will load cache (if exists) and save cache to the relativePath under the predetermined path.
        /// </summary>
        /// <param name="relativePath">relativePath under the predetermined path, the final path will be "{PATH}/{relativePath}/..."</param>
        static public void Register(Window window, string relativePath)
        {
            new WindowLocationCache(window, relativePath);
        }
    }
}

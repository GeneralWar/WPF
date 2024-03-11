using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;

namespace General.WPF
{
    public class RenderHostWindow : HwndHost
    {
        public delegate void OnWindowCreate(IntPtr handle);
        public event OnWindowCreate? onWindowCreate = null;

        public RenderHostWindow()
        {
            this.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
            this.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
        }

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            int width = 0, height = 0;
            foreach (Screen screen in Screen.AllScreens)
            {
                width = Math.Max(width, screen.Bounds.Width);
                height = Math.Max(height, screen.Bounds.Height);
            }

            IntPtr handle = CreateWindowEx(0, "static", "RenderView", WS_CHILD | WS_VISIBLE, 0, 0, width, height, hwndParent.Handle, IntPtr.Zero, Process.GetCurrentProcess().Handle, IntPtr.Zero);
            this.onWindowCreate?.Invoke(handle);
            return new HandleRef(this, handle);
        }

        //protected override void OnRender(DrawingContext drawingContext)
        //{
        //    base.OnRender(drawingContext);
        //}

        //protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        //{
        //    base.OnRenderSizeChanged(sizeInfo);
        //}

        //protected override IntPtr WndProc(IntPtr windowHandle, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
        //{
        //    if (this.Handle != windowHandle)
        //    {
        //        handled = false;
        //        return IntPtr.Zero;
        //    }

        //    handled = true;
        //    switch (message)
        //    {
        //        case WM_SIZE:
        //            break;
        //        default:
        //            handled = false;
        //            break;
        //    }
        //    return IntPtr.Zero;
        //}

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            if (IntPtr.Zero == this.Handle)
            {
                return;
            }

            DestroyWindow(this.Handle);
        }

        struct RECT
        {
#pragma warning disable 649
            public int left;
            public int top;
            public int right;
            public int bottom;
#pragma warning restore 649

            public int Width => this.right - this.left;
            public int Height => this.bottom - this.top;
        }

        #region Bindings

        private const uint WS_CHILD = 0x40000000;
        private const uint WS_VISIBLE = 0x10000000;

        private const int WM_SIZE = 0x0005;
        private const int WM_PAINT = 0x000F;

        [DllImport("user32.dll", EntryPoint = "CreateWindowExA", CallingConvention = CallingConvention.StdCall)]
        static private extern IntPtr CreateWindowEx(int dwExStyle, string lpszClassName, string lpszWindowName, uint style, int x, int y, int width, int height, IntPtr hwndParent, IntPtr hMenu, IntPtr hInst, IntPtr pvParam);

        [DllImport("user32.dll", EntryPoint = "DestroyWindow", CallingConvention = CallingConvention.StdCall)]
        static private extern bool DestroyWindow(IntPtr hwnd);

        [DllImport("user32.dll", CallingConvention = CallingConvention.StdCall)]
        static private extern bool GetWindowRect(IntPtr window, out RECT rect);

        [DllImport("kernel32.dll")]
        static private extern uint GetLastError();
        #endregion
    }
}

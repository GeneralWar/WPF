using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace General.WPF
{
    /// <summary>
    /// RenderPanel.xaml 的交互逻辑
    /// </summary>
    public partial class RenderView : UserControl
    {
        private RenderHostWindow mHostWindow;
        public RenderHostWindow HostWindow => mHostWindow;

        public event RenderHostWindow.OnWindowCreate? onWindowCreate = null;

        public RenderView()
        {
            InitializeComponent();

            RenderHostWindow view = mHostWindow = new RenderHostWindow();
            view.onWindowCreate += this.onHostWindowCreate;
            view.Focusable = true;
            mRenderHolder.Children.Add(view);
        }

        private void onHostWindowCreate(System.IntPtr handle)
        {
            this.onWindowCreate?.Invoke(handle);
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            mHostWindow.Focus();
        }
    }
}

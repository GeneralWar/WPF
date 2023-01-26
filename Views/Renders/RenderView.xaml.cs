using System.Windows;
using System.Windows.Controls;

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
            mRenderHolder.Children.Add(view);
        }

        private void onHostWindowCreate(System.IntPtr handle)
        {
            this.onWindowCreate?.Invoke(handle);
        }
    }
}

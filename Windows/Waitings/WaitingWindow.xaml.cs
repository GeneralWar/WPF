using System;
using System.Threading.Tasks;
using System.Windows;

namespace General.WPF
{
    /// <summary>
    /// WaitingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class WaitingWindow : Window
    {
        static private readonly DependencyProperty ProgressHeightProperty = DependencyProperty.Register(nameof(ProgressHeight), typeof(double), typeof(WaitingWindow), new PropertyMetadata(24.0));
        static private readonly DependencyProperty ProgressMarginProperty = DependencyProperty.Register(nameof(ProgressMargin), typeof(Thickness), typeof(WaitingWindow), new PropertyMetadata(new Thickness(6, 0, 6, 0)));
        static private readonly DependencyProperty ProgressTextProperty = DependencyProperty.Register(nameof(ProgressText), typeof(string), typeof(WaitingWindow), new PropertyMetadata(""));

        public double ProgressHeight { get => (double)this.GetValue(ProgressHeightProperty); set => this.SetValue(ProgressHeightProperty, value); }
        public Thickness ProgressMargin { get => (Thickness)this.GetValue(ProgressMarginProperty); set => this.SetValue(ProgressMarginProperty, value); }
        public string ProgressText { get => (string)this.GetValue(ProgressTextProperty); set => this.SetValue(ProgressTextProperty, value); }

        private Func<Task>? mAction = null;

        public WaitingWindow(Func<Task> action)
        {
            mAction = action;
            InitializeComponent();
        }

        protected override async void OnInitialized(EventArgs _)
        {
            base.OnInitialized(_);

            if (mAction is not null)
            {
                mProgressBar.IsIndeterminate = true;
                try
                {
                    await mAction.Invoke();
                }
                catch (Exception e)
                {
                    this.ShowErrorMessageBox(e);
                }
                await this.Dispatcher.InvokeAsync(this.Close);
                return;
            }
        }
    }
}

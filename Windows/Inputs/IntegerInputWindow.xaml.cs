using System.Windows;
using System.Windows.Controls;

namespace General.WPF
{
    /// <summary>
    /// IntegerInputWindow.xaml 的交互逻辑
    /// </summary>
    public partial class IntegerInputWindow : Window
    {
        static private readonly DependencyProperty TipProperty = DependencyProperty.Register(nameof(Tip), typeof(string), typeof(IntegerInputWindow), new PropertyMetadata("请输入一个整数"));
        public string? Tip { get => this.GetValue(TipProperty) as string; set => this.SetValue(TipProperty, value); }

        static private readonly DependencyProperty MinValueProperty = DependencyProperty.Register(nameof(MinValue), typeof(int?), typeof(IntegerInputWindow));
        public int? MinValue { get => this.GetValue(MinValueProperty) as int?; set => this.SetValue(MinValueProperty, value); }

        static private readonly DependencyProperty MaxValueProperty = DependencyProperty.Register(nameof(MaxValue), typeof(int?), typeof(IntegerInputWindow));
        public int? MaxValue { get => this.GetValue(MaxValueProperty) as int?; set => this.SetValue(MaxValueProperty, value); }

        static private readonly DependencyProperty ValueStringProperty = DependencyProperty.Register(nameof(ValueString), typeof(string), typeof(IntegerInputWindow));
        public string? ValueString { get => this.GetValue(ValueStringProperty) as string; set => this.SetValue(ValueStringProperty, value); }

        private int mTempValue;
        public int? Value { get; private set; }

        public IntegerInputWindow()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == MinValueProperty || e.Property == MaxValueProperty)
            {
                this.updateTip();
            }
        }

        private void updateTip()
        {
            if (this.MinValue.HasValue && this.MaxValue.HasValue)
            {
                this.Tip = $"请输入一个介于{this.MinValue.Value}~{this.MaxValue.Value}之间的整数";
            }
            else if (this.MinValue.HasValue)
            {
                this.Tip = $"请输入一个大于{this.MinValue.Value}的整数";
            }
            else if (this.MaxValue.HasValue)
            {
                this.Tip = $"请输入一个小于{this.MaxValue.Value}的整数";
            }
            else
            {
                this.Tip = TipProperty.DefaultMetadata.DefaultValue as string ?? "请输入一个整数";
            }
        }

        private void onLeftButtonClick(object sender, RoutedEventArgs e)
        {
            this.ValueString = (mTempValue = this.MinValue ?? int.MinValue).ToString();
        }

        private void onRightButtonClick(object sender, RoutedEventArgs e)
        {
            this.ValueString = (mTempValue = this.MaxValue ?? int.MaxValue).ToString();
        }

        private void onTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            try
            {
                TextBox? box = sender as TextBox;
                if (box is null)
                {
                    return;
                }

                string content = (box.Text ?? "").Insert(box.CaretIndex, e.Text);
                mTempValue = int.Parse(content);
            }
            catch
            {
                this.ShowMessageBox("请输入有效的整数");
                e.Handled = true;
            }
        }

        private void onEnsureButton(object sender, RoutedEventArgs e)
        {
            this.Value = mTempValue;
            this.Close();
        }
    }
}

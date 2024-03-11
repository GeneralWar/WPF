using System;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace General.WPF
{
    /// <summary>
    /// DateTimePicker.xaml 的交互逻辑
    /// </summary>
    public partial class DateTimePicker : UserControl
    {
        static private readonly DependencyProperty SelectedDateProperty = DependencyProperty.Register(nameof(SelectedDate), typeof(DateTime?), typeof(DateTimePicker));
        public DateTime? SelectedDate { get => (DateTime?)this.GetValue(SelectedDateProperty); set => this.SetValue(SelectedDateProperty, value); }

        static private readonly DependencyProperty IsTodayHighlightedProperty = DependencyProperty.Register(nameof(IsTodayHighlighted), typeof(bool), typeof(DateTimePicker));
        public bool IsTodayHighlighted { get => (bool)this.GetValue(IsTodayHighlightedProperty); set => this.SetValue(IsTodayHighlightedProperty, value); }

        static private readonly DependencyProperty HourSourceProperty = DependencyProperty.Register(nameof(HourSource), typeof(IEnumerable), typeof(DateTimePicker));
        public IEnumerable HourSource { get => (IEnumerable)this.GetValue(HourSourceProperty); set => this.SetValue(HourSourceProperty, value); }

        static private readonly DependencyProperty SelectedHourProperty = DependencyProperty.Register(nameof(SelectedHour), typeof(int), typeof(DateTimePicker));
        public int SelectedHour { get => (int)this.GetValue(SelectedHourProperty); set => this.SetValue(SelectedHourProperty, value); }

        static private readonly DependencyProperty MinuteSourceProperty = DependencyProperty.Register(nameof(MinuteSource), typeof(IEnumerable), typeof(DateTimePicker));
        public IEnumerable MinuteSource { get => (IEnumerable)this.GetValue(MinuteSourceProperty); set => this.SetValue(MinuteSourceProperty, value); }

        static private readonly DependencyProperty SelectedMinuteProperty = DependencyProperty.Register(nameof(SelectedMinute), typeof(int), typeof(DateTimePicker));
        public int SelectedMinute { get => (int)this.GetValue(SelectedMinuteProperty); set => this.SetValue(SelectedMinuteProperty, value); }

        static private readonly DependencyProperty SecondSourceProperty = DependencyProperty.Register(nameof(SecondSource), typeof(IEnumerable), typeof(DateTimePicker));
        public IEnumerable SecondSource { get => (IEnumerable)this.GetValue(SecondSourceProperty); set => this.SetValue(SecondSourceProperty, value); }

        static private readonly DependencyProperty SelectedSecondProperty = DependencyProperty.Register(nameof(SelectedSecond), typeof(int), typeof(DateTimePicker));
        public int SelectedSecond { get => (int)this.GetValue(SelectedSecondProperty); set => this.SetValue(SelectedSecondProperty, value); }

        static private readonly DependencyProperty SelectedDateTimeProperty = DependencyProperty.Register(nameof(SelectedDateTime), typeof(DateTime), typeof(DateTimePicker));
        public DateTime SelectedDateTime { get => (DateTime)this.GetValue(SelectedDateTimeProperty); set => this.SetValue(SelectedDateTimeProperty, value); }

        static private readonly DependencyProperty SecondVisibleTimeProperty = DependencyProperty.Register(nameof(SecondVisible), typeof(bool), typeof(DateTimePicker));
        public bool SecondVisible { get => (bool)this.GetValue(SecondVisibleTimeProperty); set => this.SetValue(SecondVisibleTimeProperty, value); }

        public DateTimePicker()
        {
            this.HourSource = Enumerable.Range(0, 24);
            this.MinuteSource = Enumerable.Range(0, 59);
            this.SecondSource = Enumerable.Range(0, 59);
            InitializeComponent();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == SelectedDateProperty || e.Property == SelectedHourProperty || e.Property == SelectedMinuteProperty || e.Property == SelectedSecondProperty)
            {
                this.SelectedDateTime = (this.SelectedDate ?? new DateTime()).AddHours(this.SelectedHour).AddMinutes(this.SelectedMinute).AddSeconds(this.SelectedSecond);
            }
            else if (e.Property == SelectedDateTimeProperty)
            {
                DateTime value = (DateTime)e.NewValue;
                this.SelectedDate = value.Date;
                this.SelectedHour = value.Hour;
                this.SelectedMinute = value.Minute;
                this.SelectedSecond = value.Second;
            }
        }
    }
}

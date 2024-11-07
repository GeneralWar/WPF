using System;
using System.Windows.Controls;

namespace General.WPF
{
    public class EnumValueItem<EnumValue> : ContentControl where EnumValue : struct, Enum
    {
        public EnumValue Value { get; }
        public string Text { get; }

        public EnumValueItem(EnumValue value) : this(value, value.ToString()) { }

        public EnumValueItem(EnumValue value, string text)
        {
            this.Value = value;
            this.Text = text;
            this.Content = text;
        }
    }
}

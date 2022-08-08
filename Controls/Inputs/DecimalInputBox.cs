using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace General.WPF
{
    public partial class DecimalInputBox : TextBox
    {
        public delegate void OnValueChange(DecimalInputBox input, decimal value);

        static protected readonly DependencyProperty PROPERTY_VALUE = DependencyProperty.Register(nameof(DecimalInputBox.Value), typeof(decimal), typeof(DecimalInputBox));

        public int Precision { get; set; } = 2;
        protected string StringFormat => $"F{this.Precision}";

        public decimal Value { get => (decimal)this.GetValue(PROPERTY_VALUE); set { this.SetValue(PROPERTY_VALUE, value); this.updateText(value); } }

        public event OnValueChange? ValueChange = null;

        public DecimalInputBox()
        {
            InputScope scope = new InputScope();
            scope.Names.Add(new InputScopeName { NameValue = InputScopeNameValue.Number });
            this.InputScope = scope;
        }

        /// <summary>
        /// Check text for current control
        /// </summary>
        /// <param name="value">The real value that current control holds</param>
        /// <returns>The display text for TextBox</returns>
        protected virtual string checkString(decimal value)
        {
            return value.ToString(this.StringFormat);
        }

        private void updateText(decimal value)
        {
            this.Text = this.checkString(value);
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);

            decimal value;
            if (!decimal.TryParse(this.Text, out value))
            {
                this.updateText(this.Value);
                return;
            }

            decimal finalValue = this.checkValue(value);
            string expectedString = this.checkString(finalValue);
            if (this.Text != expectedString)
            {
                int index = expectedString.IndexOf(this.Text);
                if (e.Changes.All(c => c.AddedLength > 0 && c.RemovedLength > 0))
                {
                    ++index;
                }
                if (index < 1)
                {
                    index = this.CaretIndex;
                }
                this.Text = expectedString;
                this.CaretIndex = index;
                return;
            }

            this.Value = finalValue;
            this.ValueChange?.Invoke(this, this.Value);
        }

        /// <summary>
        /// Check final value for current control
        /// </summary>
        /// <param name="valueFromText">Value parsed from TextBox</param>
        /// <returns>The final value calculated by current control</returns>
        protected virtual decimal checkValue(decimal valueFromText)
        {
            return valueFromText;
        }
    }
}

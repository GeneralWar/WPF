using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace General.WPF
{
    public partial class NumberInputBox : TextBox
    {
        public delegate void OnValueChange(NumberInputBox input, double value);

        static protected readonly DependencyProperty PROPERTY_VALUE = DependencyProperty.Register(nameof(NumberInputBox.Value), typeof(double), typeof(NumberInputBox));

        public int Precision { get; set; } = 2;
        protected string StringFormat => $"F{this.Precision}";

        public double Value { get => (double)this.GetValue(PROPERTY_VALUE); set { this.SetValue(PROPERTY_VALUE, value); this.updateText(value); } }

        public event OnValueChange? ValueChange = null;

        public NumberInputBox()
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
        protected virtual string checkString(double value)
        {
            return value.ToString(this.StringFormat);
        }

        private void updateText(double value)
        {
            Tracer.Assert(0 == value || double.IsNormal(value));
            this.Text = this.checkString(value);
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);

            double value;
            if (!double.TryParse(this.Text, out value))
            {
                this.updateText(this.Value);
                return;
            }

            double finalValue = this.checkValue(value);
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
        protected virtual double checkValue(double valueFromText)
        {
            return valueFromText;
        }
    }
}

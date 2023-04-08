using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace General.WPF
{
    public partial class DecimalInputBox : NumberInputBox<decimal>
    {
        public delegate void OnValueChange(DecimalInputBox input, decimal value);
        public event OnValueChange? ValueChanging = null;
        /// <summary>
        /// Only report data from text when user press Enter or this control lost focus, 
        /// cached data will not update, and will reset text with cached data before this event
        /// </summary>
        public event OnValueChange? ValueChanged = null;

        public int Precision { get; set; } = 2;
        protected string StringFormat => $"F{this.Precision}";

        public DecimalInputBox()
        {
            InputScope scope = new InputScope();
            scope.Names.Add(new InputScopeName { NameValue = InputScopeNameValue.Number });
            this.InputScope = scope;
        }

        protected override bool TryParse(string text, out decimal value)
        {
            return decimal.TryParse(text, out value);
        }

        protected override string checkString(decimal value)
        {
            return value.ToString(this.StringFormat);
        }

        protected override decimal checkValue(decimal valueFromText)
        {
            return valueFromText;
        }

        protected override void reportValueChanging(decimal value)
        {
            this.ValueChanging?.Invoke(this, value);
        }

        protected override void reportValueChanged(decimal value)
        {
            this.ValueChanged?.Invoke(this, value);
        }
    }
}

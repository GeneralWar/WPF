using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace General.WPF
{
    public partial class IntegerInputBox : NumberInputBox<int>
    {
        public delegate void OnValueChange(IntegerInputBox input, int value);
        public event OnValueChange? ValueChanging = null;
        /// <summary>
        /// Only report data from text when user press Enter or this control lost focus, 
        /// cached data will not update, and will reset text with cached data before this event
        /// </summary>
        public event OnValueChange? ValueChanged = null;

        public IntegerInputBox()
        {
            InputScope scope = new InputScope();
            scope.Names.Add(new InputScopeName { NameValue = InputScopeNameValue.Number });
            this.InputScope = scope;
        }

        protected override bool TryParse(string text, out int value)
        {
            return int.TryParse(text, out value);
        }

        protected override string checkString(int value)
        {
            return value.ToString();
        }

        protected override int checkValue(int valueFromText)
        {
            return valueFromText;
        }

        protected override void reportValueChanging(int value)
        {
            this.ValueChanging?.Invoke(this, value);
        }

        protected override void reportValueChanged(int value)
        {
            this.ValueChanged?.Invoke(this, value);
        }
    }
}

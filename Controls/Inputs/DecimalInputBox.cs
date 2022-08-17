using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace General.WPF
{
    public partial class DecimalInputBox : NumberInputBox<decimal>
    {
        public delegate void OnValueChange(DecimalInputBox input, decimal value);

        public int Precision { get; set; } = 2;
        protected string StringFormat => $"F{this.Precision}";

        public event OnValueChange? ValueChange = null;

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

        protected override void reportValueChange(decimal value)
        {
            this.ValueChange?.Invoke(this, value);
        }
    }
}

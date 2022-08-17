using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace General.WPF
{
    public partial class IntegerInputBox : NumberInputBox<int>
    {
        public delegate void OnValueChange(IntegerInputBox input, int value);

        public event OnValueChange? ValueChange = null;

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

        protected override void reportValueChange(int value)
        {
            this.ValueChange?.Invoke(this, value);
        }
    }
}

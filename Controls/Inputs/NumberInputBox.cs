using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace General.WPF
{
    public abstract class NumberInputBox<T> : TextBox where T : struct
    {
        static protected readonly DependencyProperty PROPERTY_VALUE = DependencyProperty.Register(nameof(NumberInputBox<T>.Value), typeof(T), typeof(NumberInputBox<T>), new FrameworkPropertyMetadata(default(T), OnValuePropertyChange)); 
        
        static private void OnValuePropertyChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumberInputBox<T>? input = d as NumberInputBox<T>;
            if (input is null)
            {
                return;
            }

            input.Value = (T)e.NewValue;
        }

        public T Value { get => (T)this.GetValue(PROPERTY_VALUE); set { this.SetValue(PROPERTY_VALUE, value); this.updateText(value, null); } }

        public event RoutedEventHandler? EnterDown = null;

        public NumberInputBox()
        {
            InputScope scope = new InputScope();
            scope.Names.Add(new InputScopeName { NameValue = InputScopeNameValue.Number });
            this.InputScope = scope;
            this.VerticalAlignment = VerticalAlignment.Center;
            this.VerticalContentAlignment = VerticalAlignment.Center;
        }

        /// <summary>
        /// Check text for current control
        /// </summary>
        /// <param name="value">The real value that current control holds</param>
        /// <returns>The display text for TextBox</returns>
        protected abstract string checkString(T value);

        protected abstract bool TryParse(string text, out T value);

        protected override void OnTextInput(TextCompositionEventArgs e)
        {
            base.OnTextInput(e);

            if ("." == e.Text)
            {
                int index = this.Text.IndexOf(e.Text);
                if (index > -1)
                {
                    this.CaretIndex = index + 1;
                }
            }
        }

        private void updateText(T value, TextChangedEventArgs? e)
        {
            string targetText = this.checkString(value);
            string currentText = this.Text;
            this.Text = targetText;
            if (e is not null)
            {
                if (0 == e.Changes.Count)
                {
                    return;
                }
                if (1 != e.Changes.Count)
                {
                    throw new NotImplementedException();
                }

                TextChange change = e.Changes.ElementAt(0);
                string insertedText = currentText.Substring(change.Offset, change.AddedLength);
                if (insertedText == currentText)
                {
                    this.CaretIndex = insertedText.Length;
                }
                else
                {
                    string movedText = currentText.Substring(change.Offset + change.AddedLength, change.AddedLength);
                    if (movedText == insertedText)
                    {
                        this.CaretIndex = change.Offset + change.AddedLength;
                    }
                }
            }
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);

            T value;
            if (!this.TryParse(this.Text, out value))
            {
                this.updateText(this.Value, e);
                return;
            }

            T finalValue = this.checkValue(value);
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

            if (!this.Value.Equals(finalValue))
            {
                this.Value = finalValue;
                this.reportValueChange(finalValue);
            }
        }

        /// <summary>
        /// Check final value for current control
        /// </summary>
        /// <param name="valueFromText">Value parsed from TextBox</param>
        /// <returns>The final value calculated by current control</returns>
        protected abstract T checkValue(T valueFromText);

        protected abstract void reportValueChange(T value);

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (Key.Enter == e.Key)
            {
                this.EnterDown?.Invoke(this, e);
            }
        }
    }

    public class NumberInputBox : NumberInputBox<double>
    {
        public delegate void OnValueChange(NumberInputBox input, double value);

        public int Precision { get; set; } = 2;
        protected string StringFormat => $"F{this.Precision}";

        public event OnValueChange? ValueChange = null;

        public NumberInputBox()
        {
            InputScope scope = new InputScope();
            scope.Names.Add(new InputScopeName { NameValue = InputScopeNameValue.Number });
            this.InputScope = scope;
        }

        protected override bool TryParse(string text, out double value)
        {
            return double.TryParse(text, out value);
        }

        protected override string checkString(double value)
        {
            return value.ToString(this.StringFormat);
        }

        protected override double checkValue(double valueFromText)
        {
            return valueFromText;
        }

        protected override void reportValueChange(double value)
        {
            this.ValueChange?.Invoke(this, value);
        }
    }
}

using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace General.WPF
{
    public abstract class NumberInputBox<ValueType> : TextBox where ValueType : struct
    {
        static protected readonly DependencyProperty PROPERTY_VALUE = DependencyProperty.Register(nameof(NumberInputBox<ValueType>.Value), typeof(ValueType), typeof(NumberInputBox<ValueType>), new FrameworkPropertyMetadata(default(ValueType), OnValuePropertyChange));

        static private void OnValuePropertyChange(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            NumberInputBox<ValueType>? input = d as NumberInputBox<ValueType>;
            if (input is null)
            {
                return;
            }

            input.Value = (ValueType)e.NewValue;
        }

        public ValueType Value { get => (ValueType)this.GetValue(PROPERTY_VALUE); set { this.updateData(value); this.updateText(value, true, null); } }
        private bool mTextUpdating = false;

        public NumberInputBox()
        {
            InputScope scope = new InputScope();
            scope.Names.Add(new InputScopeName { NameValue = InputScopeNameValue.Number });
            this.InputScope = scope;
            this.VerticalAlignment = VerticalAlignment.Center;
            this.VerticalContentAlignment = VerticalAlignment.Center;

            this.updateText(this.Value, true, null);
        }

        /// <summary>
        /// Check text for current control
        /// </summary>
        /// <param name="value">The real value that current control holds</param>
        /// <returns>The display text for TextBox</returns>
        protected abstract string checkString(ValueType value);

        protected abstract bool TryParse(string text, out ValueType value);

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

        private string shortenText(string text)
        {
            return text.TrimEnd('0').TrimEnd('.');
        }

        private void updateText(string text, bool trim)
        {
            mTextUpdating = true;
            this.Text = trim ? this.shortenText(text) : text;
            mTextUpdating = false;
        }

        private void updateText(ValueType value, bool trim, TextChangedEventArgs? e)
        {
            string currentText = this.Text;
            string targetText = this.checkString(value);
            this.updateText(targetText, trim);

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

        private void updateData(ValueType value)
        {
            this.SetValue(PROPERTY_VALUE, value);
        }

        private void updateDataChanging(ValueType value)
        {
            if (this.Value.Equals(value))
            {
                return;
            }

            this.updateData(value);
            this.reportValueChanging();
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);

            if (mTextUpdating)
            {
                return;
            }

            TextChange change = e.Changes.First();

            ValueType value;
            string text = this.Text;
            if (string.IsNullOrWhiteSpace(text))
            {
                this.updateDataChanging(default(ValueType));
                this.updateText("", false);
                return;
            }

            if (!this.TryParse(text, out value))
            {
                if (text.Count('-') == text.Length)
                {
                    text = "-";
                }

                if ("-" == text)
                {
                    this.updateDataChanging(value);
                    this.updateText(text, false);
                    this.CaretIndex = text.Length;
                    return;
                }

                if (text.EndsWith('.') && text.Count('.') > 1)
                {
                    text = text.Substring(0, text.Length - 1);
                    this.updateDataChanging(value);
                    this.updateText(text, false);
                    this.CaretIndex = text.Length;
                    return;
                }

                string previousText = text.Remove(change.Offset, change.AddedLength);
                if (this.TryParse(previousText, out value))
                {
                    this.updateText(previousText, false);
                    this.CaretIndex = previousText.Length;
                    return;
                }

                throw new NotImplementedException();
                //this.updateText(this.Value, false, e);
                //return;
            }

            ValueType finalValue = this.checkValue(value);
            //string expectedString = this.shortenText(this.checkString(finalValue));
            //if (this.Text != expectedString)
            //{
            //    if (this.Text.EndsWith('.') && 1 == this.Text.Count('.'))
            //    {
            //        return;
            //    }

            //    int index = expectedString.IndexOf(this.Text);
            //    if (e.Changes.All(c => c.AddedLength > 0 && c.RemovedLength > 0))
            //    {
            //        ++index;
            //    }
            //    if (index < 1)
            //    {
            //        index = this.CaretIndex;
            //    }
            //    this.Text = expectedString;
            //    this.CaretIndex = index;
            //    return;
            //}

            this.updateDataChanging(finalValue);

            int caretIndex = change.Offset + change.AddedLength;
            if (text.StartsWith('0') && "0" != text)
            {
                string shortText = text.TrimStart('0');
                caretIndex -= text.Length - shortText.Length;
                text = shortText;
            }
            this.updateText(text, false);
            this.CaretIndex = Math.Max(0, caretIndex);
        }

        /// <summary>
        /// Check final value for current control
        /// </summary>
        /// <param name="valueFromText">Value parsed from TextBox</param>
        /// <returns>The final value calculated by current control</returns>
        protected abstract ValueType checkValue(ValueType valueFromText);

        protected abstract void reportValueChanging();
        protected abstract void reportValueChanged();

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (Key.Enter == e.Key)
            {
                FrameworkElement? ancestor = this.FindAncestor<FrameworkElement>(false, e => e.Focusable);
                if (ancestor is null || (ancestor is Window && this.GetTopWindow() == ancestor))
                {
                    this.updateText(this.Value, true, null);
                    this.CaretIndex = this.Text.Length;
                    this.reportValueChanged();
                }
                else
                {
                    ancestor.Focus();
                }
            }
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            this.updateText(this.Value, true, null);
            this.reportValueChanged();
        }
    }

    public class NumberInputBox : NumberInputBox<double>
    {
        public int Precision { get; set; } = 2;
        protected string StringFormat => $"F{this.Precision}";

        public delegate void OnValueChange(NumberInputBox input, double value);
        public event OnValueChange? ValueChanging = null;
        public event OnValueChange? ValueChanged = null;

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

        protected override void reportValueChanging()
        {
            this.ValueChanging?.Invoke(this, this.Value);
        }

        protected override void reportValueChanged()
        {
            this.ValueChanged?.Invoke(this, this.Value);
        }
    }
}

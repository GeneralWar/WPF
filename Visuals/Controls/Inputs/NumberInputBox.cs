using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace General.WPF
{
    internal class DefaultNumberInputValueTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() ?? "0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double d;
            double.TryParse(value as string ?? "", out d);
            return d;
        }
    }

    public abstract class NumberInputBox<ValueType> : TextBox where ValueType : struct
    {
        static public readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(NumberInputBox<ValueType>.Value), typeof(ValueType), typeof(NumberInputBox<ValueType>));

        public ValueType Value { get => (ValueType)this.GetValue(ValueProperty); set { this.updateData(value); this.updateText(value, true, null); } }
        private bool mTextUpdating = false;

        static public readonly DependencyProperty AutoSetValueWhenChangedProperty = DependencyProperty.Register(nameof(NumberInputBox<ValueType>.AutoSetValueWhenChanged), typeof(bool), typeof(NumberInputBox<ValueType>), new PropertyMetadata(true));
        public bool AutoSetValueWhenChanged { get => (bool)this.GetValue(AutoSetValueWhenChangedProperty); set { this.SetValue(AutoSetValueWhenChangedProperty, value); } }

        public event RoutedEventHandler? EnterDown = null;

        public NumberInputBox()
        {
            this.VerticalAlignment = VerticalAlignment.Center;
            this.VerticalContentAlignment = VerticalAlignment.Center;

            Binding binding = new Binding();
            binding.Source = this;
            binding.Path = new PropertyPath(ValueProperty.Name);
            binding.Converter = this.getValueTextConverter();
            this.SetBinding(TextProperty, binding);

            this.updateText(this.Value, true, null);
        }

        protected virtual IValueConverter getValueTextConverter()
        {
            return new DefaultNumberInputValueTextConverter(); ;
        }

        /// <summary>
        /// Check text for current control
        /// </summary>
        /// <param name="value">The real value that current control holds</param>
        /// <returns>The display text for TextBox</returns>
        protected abstract string checkString(ValueType value);

        protected abstract bool TryParse(string text, out ValueType value);

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            base.OnPreviewTextInput(e);

            /// InputScope cannot allow both digits and negative value, so handle text input here

            string text = this.Text.Insert(this.CaretIndex, e.Text);

            ValueType value;
            if (!this.TryParse(text, out value))
            {
                e.Handled = true;
                return;
            }
        }

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
            if ("0" == text)
            {
                return text;
            }
            return text.Contains('.') ? text.TrimEnd('0').TrimEnd('.') : text;
        }

        /// <summary>
        /// update text only, without emiting value change event
        /// </summary>
        /// <param name="text"></param>
        public void UpdateText(string text)
        {
            this.updateText(text, true);
        }

        private void updateText(string text, bool trim)
        {
            mTextUpdating = true;
            this.Dispatcher.Invoke(() => this.Text = trim ? this.shortenText(text) : text);
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
            this.SetValue(ValueProperty, value);
        }

        private void updateDataChanging(ValueType value)
        {
            if (this.Value.Equals(value))
            {
                return;
            }

            //this.updateData(value); do not update data manually, to prevent removing binding expressions
            this.reportValueChanging(value);
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

                if ("." == text)
                {
                    this.updateDataChanging(default(ValueType));
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

            this.updateDataChanging(value);

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

        protected abstract void reportValueChanging(ValueType value);
        protected abstract void reportValueChanged(ValueType value);

        private ValueType checkValueFromText()
        {
            ValueType value;
            return this.TryParse(this.Text, out value) ? value : this.Value;
        }

        private void reportValueChanged()
        {
            ValueType value = this.checkValueFromText();
            this.updateText(this.Value, true, null);
            this.reportValueChanged(value);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (Key.Enter == e.Key)
            {
                FrameworkElement? ancestor = this.FindAncestor<FrameworkElement>(false, e => e.Focusable);
                if (ancestor is null || (ancestor is Window && this.GetTopWindow() == ancestor))
                {
                    ValueType value;
                    string text = this.Text;
                    this.reportValueChanged();
                    this.CaretIndex = this.Text.Length;
                    if (this.AutoSetValueWhenChanged && this.TryParse(text, out value))
                    {
                        this.Value = value;
                    }
                }
                else
                {
                    ancestor.Focus();
                }

                this.EnterDown?.Invoke(this, e);
            }
        }

        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            this.reportValueChanged();
        }

        public override string ToString()
        {
            return $"{this.GetType().FullName}: {this.Value}";
        }
    }

    public class NumberInputBox : NumberInputBox<double>
    {
        public int Precision { get; set; } = 2;
        protected string StringFormat => $"F{this.Precision}";

        public delegate void OnValueChange(NumberInputBox input, double value);
        public event OnValueChange? ValueChanging = null;
        /// <summary>
        /// Only report data from text when user press Enter or this control lost focus, 
        /// cached data will not update, and will reset text with cached data before this event
        /// </summary>
        public event OnValueChange? ValueChanged = null;

        protected override bool TryParse(string text, out double value)
        {
            return double.TryParse(text, out value);
        }

        protected override string checkString(double value)
        {
            return value.ToString(this.StringFormat);
        }

        protected override void reportValueChanging(double value)
        {
            this.ValueChanging?.Invoke(this, value);
        }

        protected override void reportValueChanged(double value)
        {
            this.ValueChanged?.Invoke(this, value);
        }
    }
}

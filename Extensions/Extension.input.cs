using System.Windows.Input;

static public partial class WPFExtension
{
    static public bool IsControl(this Key value) => Key.LeftCtrl == value || Key.RightCtrl == value;
    static public bool IsShift(this Key value) => Key.LeftShift == value || Key.RightShift == value;
    static public bool IsAlt(this Key value) => Key.LeftAlt == value || Key.RightAlt == value;

    static public bool IsIme(this Key value) => (Key.ImeConvert <= value && value <= Key.ImeModeChange) || Key.ImeProcessed == value;
    static public bool IsLetter(this Key value) => Key.A <= value && value <= Key.Z;
    static public bool IsDigit(this Key value) => (Key.D0 <= value && value <= Key.D9) || (Key.NumPad0 <= value && value <= Key.NumPad9);
    static public bool IsLetterOrDigit(this Key value) => IsLetter(value) || IsDigit(value);
}

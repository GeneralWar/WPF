using System;
using System.Diagnostics;
using System.Drawing;

namespace General.WPF
{
    /* No LengthConverter here because it cannot convert pixels to other units */
    public struct PixelLength
    {
        static private readonly Lazy<Graphics> LazyGraphics = new Lazy<Graphics>(() => Graphics.FromHwnd(Process.GetCurrentProcess().MainWindowHandle));
        static private Graphics Graphics => LazyGraphics.Value;

        public double Value { get; }

        public PixelLength(double value)
        {
            this.Value = value;
        }

        public double ToCentimeters() => this.ToInches() * 2.54d; // 1in = 2.54cm

        public double ToPoints() => this.ToInches() * 72d; // 1in = 72pt

        public double ToInches() => this.Value / Graphics.DpiX;

        public override string ToString() => this.Value + "px";

        static public PixelLength FromCentimeters(double value) => FromInches(value / 2.54d);
        static public PixelLength FromPoints(double value) => FromInches(value / 72d);

        static public PixelLength FromInches(double value) => new PixelLength(value * Graphics.DpiX);

        /// <summary>
        /// convert size in other units to pixels, such as 'in', 'cm', 'pt' or pure number in pixels
        /// </summary>
        /// <param name="source">size in other units, such as '2.5in', '3.6cm', '4.7pt'</param>
        /// <returns></returns>
        static public PixelLength FromString(string value)
        {
            value = value.Trim();
            if (value.EndsWith("cm"))
            {
                return FromCentimeters(double.Parse(value.Substring(0, value.Length - 2)));
            }
            if (value.EndsWith("pt"))
            {
                return FromPoints(double.Parse(value.Substring(0, value.Length - 2)));
            }
            if (value.EndsWith("in"))
            {
                return FromInches(double.Parse(value.Substring(0, value.Length - 2)));
            }
            return new PixelLength(double.Parse(value));
        }
    }
}

using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

static public partial class WPFExtension
{
    static public ImageSource ToImageSource(this Icon icon)
    {
        return Imaging.CreateBitmapSourceFromHIcon(icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
    }

    static public bool Save(this ImageSource image, string filename)
    {
        BitmapSource? source = image as BitmapSource;
        if (source is null)
        {
            return false;
        }

        using (FileStream stream = new FileStream(filename, FileMode.Create))
        {
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(source));
            encoder.Save(stream);
        }
        return true;
    }
}

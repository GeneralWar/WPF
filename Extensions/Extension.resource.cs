using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;

static public partial class WPFExtension
{
    /// <summary>
    /// Get builtin image from assembly
    /// </summary>
    /// <param name="url">such as "ProjectName.Folder1.Folder2.ResourceName.type"</param>
    static public BitmapImage? GetBitmap(this Assembly assembly, string url)
    {
        Stream? stream = assembly.GetResource(url);
        if (stream is null)
        {
            return null;
        }

        BitmapImage image = new BitmapImage();
        image.BeginInit();
        image.StreamSource = stream;
        image.EndInit();
        stream.Close();
        return image;
    }
}

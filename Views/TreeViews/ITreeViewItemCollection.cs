using System.Windows.Controls;

namespace General.WPF
{
    public interface ITreeViewItemCollection
    {
        ItemCollection Items { get; }

        object Tag { get; }
    }
}

using System.Windows.Controls;

namespace General.WPF
{
    public interface ITreeViewItemCollection
    {
        ITreeViewItemCollection? Parent { get; }
        int SiblingIndex { get; }

        bool AllowItemsDrag { get; }
        ItemCollection Items { get; }

        object Tag { get; }
    }
}

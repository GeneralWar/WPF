using System.Windows;

namespace General.WPF
{
    static public partial class Extension
    {
        static public TabPanel? GetTabPanelUpward(this TabItem element)
        {
            FrameworkElement? parent = element as FrameworkElement;
            while (parent is not null && (parent = parent.GetRealParent()) is not TabPanel) ;
            return parent as TabPanel;
        }
    }
}

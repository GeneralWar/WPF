using General.WPF;
using System.Windows;

static public partial class WPFExtension
{
    static public TabPanel? GetTabPanelUpward(this TabItem element)
    {
        FrameworkElement? parent = element as FrameworkElement;
        while (parent is not null && (parent = parent.GetRealParent()) is not TabPanel) ;
        return parent as TabPanel;
    }
}

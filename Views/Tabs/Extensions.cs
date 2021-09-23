using System.Windows;

namespace General.WPF
{
    static public partial class Extensions
    {
        static public TabPanel GetTabPanelUpward(this TabItem element)
        {
            FrameworkElement parent = element as FrameworkElement;
            while (parent is not TabPanel && parent.Parent is not null)
            {
                parent = parent.Parent as FrameworkElement;
            }
            while (parent is not TabPanel && parent.TemplatedParent is not null)
            {
                parent = parent.TemplatedParent as FrameworkElement;
            }
            return parent as TabPanel;
        }
    }
}

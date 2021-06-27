using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace General.WPF
{
    /// <summary>
    /// TabItem.xaml 的交互逻辑
    /// </summary>
    public partial class TabItem : System.Windows.Controls.TabItem
    {
        public TabItem()
        {
            InitializeComponent();
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            base.OnPreviewMouseMove(e);

            if (MouseButtonState.Pressed == e.LeftButton)
            {
                DragDrop.DoDragDrop(this, this, DragDropEffects.Move);
            }
        }
    }
}

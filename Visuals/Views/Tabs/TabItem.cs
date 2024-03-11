using System.Windows;
using System.Windows.Input;

namespace General.WPF
{
    public class TabItem : System.Windows.Controls.TabItem
    {
        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            base.OnPreviewMouseMove(e);

            TabPanel? parent = this.FindAncestor<TabPanel>();
            if (parent is null || !parent.DragEnabled)
            {
                return;
            }

            if (MouseButtonState.Pressed == e.LeftButton)
            {
                IInputElement input = this.InputHitTest(e.GetPosition(this));
                if (null == input)
                {
                    return;
                }
                //Trace.Assert(e.Source == e.OriginalSource);
                DragDrop.DoDragDrop(this, this.ToDragData(), DragDropEffects.Move);
            }
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace General.WPF
{
    /// <summary>
    /// DataGridCheckBoxColumn.xaml 的交互逻辑
    /// </summary>
    public partial class DataGridCheckBoxColumn : System.Windows.Controls.DataGridCheckBoxColumn
    {
        public DataGridCheckBoxColumn()
        {
            InitializeComponent();
        }

        protected override FrameworkElement GenerateElement(System.Windows.Controls.DataGridCell cell, object dataItem)
        {
            CheckBox? checkBox = cell?.Content as CheckBox;
            if (checkBox is null)
            {
                checkBox = new CheckBox();
            }

            //checkBox.IsThreeState = IsThreeState;

            if (this.Binding is null)
            {
                BindingOperations.ClearBinding(checkBox, CheckBox.IsCheckedProperty);
            }
            else
            {
                BindingOperations.SetBinding(checkBox, CheckBox.IsCheckedProperty, this.Binding);
            }

            return checkBox;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
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
    /// TabPanel.xaml 的交互逻辑
    /// </summary>
    public partial class TabPanel : UserControl
    {
        public const double GRID_SPLITTER_WEIGHT = 2;

        static public readonly DependencyProperty ItemsProperty = DependencyProperty.Register("ItemsProperty", typeof(UIElementCollection), typeof(TabPanel), new PropertyMetadata(null));
        /// <summary>
        /// Gets or sets items for the TabPanel
        /// </summary>
        public UIElementCollection Items { get { return GetValue(ItemsProperty) as UIElementCollection; } set { SetValue(ItemsProperty, value); } }

        public HashSet<TabControl> mTabControls = new HashSet<TabControl>();

        public TabPanel()
        {
            InitializeComponent();
            this.Items = new UIElementCollection(this, this);
            this.addTabControl(mTabControl);
        }

        private void addTabControl(TabControl control)
        {
            mTabControls.Add(control);
            control.DragEnter += this.onDragEnter;
            control.DragLeave += this.onDragLeave;
            control.DragOver += this.onDragOver;
            control.Drop += this.onDrop;
        }

        private void checkDragPosition(TabControl control, DragEventArgs e, out double x, out double y, out double offsetX, out double offsetY, out Rect rect, out Dock direction)
        {
            Point position = e.GetPosition(control);
            x = position.X / control.ActualWidth;
            y = position.Y / control.ActualHeight;
            offsetX = Math.Min(x, 1.0 - x);
            offsetY = Math.Min(y, 1.0 - y);

            if (offsetX > .25 && offsetY > .25)
            {
                direction = (Dock)(-1);
                rect = new Rect();
                return;
            }

            if (offsetX < offsetY)
            {
                double halfWidth = control.ActualWidth * .5;
                if (x <= .25)
                {
                    direction = Dock.Left;
                    rect = new Rect(0, 0, halfWidth, control.ActualHeight);
                }
                else
                {
                    direction = Dock.Right;
                    rect = new Rect(halfWidth, 0, halfWidth, control.ActualHeight);
                }
            }
            else
            {
                double halfHeight = control.ActualHeight * .5;
                if (y <= .25)
                {
                    direction = Dock.Top;
                    rect = new Rect(0, 0, control.ActualWidth, halfHeight);
                }
                else
                {
                    direction = Dock.Bottom;
                    rect = new Rect(0, halfHeight, control.ActualWidth, halfHeight);
                }
            }
            rect.Offset(control.GetVisualOffset());
        }

        private void showSlotMask()
        {
            mSlotMask.Visibility = Visibility.Visible;
        }

        private void hideSlotMask()
        {
            mSlotMask.Visibility = Visibility.Hidden;
        }

        private void onDragEnter(object sender, DragEventArgs e)
        {
            this.onDragOver(sender, e);
        }

        private void onDragLeave(object sender, DragEventArgs e)
        {
            this.hideSlotMask();
        }

        private void onDragOver(object sender, DragEventArgs e)
        {
            Rect rect;
            Dock direction;
            double x, y, offsetX, offsetY;
            this.checkDragPosition(sender as TabControl, e, out x, out y, out offsetX, out offsetY, out rect, out direction);

            if (offsetX > .25 && offsetY > .25)
            {
                this.hideSlotMask();
                return;
            }

            this.showSlotMask();
            mSlotMask.Width = rect.Width;
            mSlotMask.Height = rect.Height;
            mSlotMask.Margin = new Thickness(rect.X, rect.Y, 0, 0);
        }

        private void onDrop(object sender, DragEventArgs e)
        {
            this.hideSlotMask();

            Rect rect;
            Dock direction;
            double x, y, offsetX, offsetY;
            this.checkDragPosition(sender as TabControl, e, out x, out y, out offsetX, out offsetY, out rect, out direction);

            TabItem item = e.Data.GetData(typeof(TabItem)) as TabItem;
            TabControl parent = item.Parent as TabControl;
            Window window = item.GetTopWindow();

            if (offsetX > .25 && offsetY > .25)
            {
                this.createNewWindow(e);
                goto RESULT;
            }

            switch (direction)
            {
                case Dock.Left:
                case Dock.Right:
                    this.seperateInColumn(sender as TabControl, e, direction);
                    break;
            }

        RESULT:
            if (0 == parent?.Items.Count)
            {
                window?.Close();
            }
        }

        private void createNewWindow(DragEventArgs e)
        {
            TabItem item = e.Data.GetData(typeof(TabItem)) as TabItem;
            if (null == item)
            {
                return;
            }

            TabControl collection = item.Parent as TabControl;
            //Trace.Assert(this == parent);
            collection.Items.Remove(item);

            Window window = new Window();
            window.Width = collection.ActualWidth + window.BorderThickness.Left + window.BorderThickness.Right;
            window.Height = collection.ActualHeight + window.BorderThickness.Top + window.BorderThickness.Bottom + 30; // Height of title bar = 30

            TabPanel parent = new TabPanel();
            window.Content = parent;

            parent.Items.Add(item);

            Point mousePosition = this.PointToScreen(e.GetPosition(this));
            window.Left = mousePosition.X;
            window.Top = mousePosition.Y;
            window.Show();
        }

        private void addColumnDefinition(Grid grid, double width)
        {
            ColumnDefinition definition = new ColumnDefinition();
            definition.Width = new GridLength(width, GridUnitType.Star);
            grid.ColumnDefinitions.Add(definition);
        }

        private void addColumnSplitter(Grid grid)
        {
            ColumnDefinition definition = new ColumnDefinition();
            definition.Width = new GridLength(GRID_SPLITTER_WEIGHT, GridUnitType.Pixel);

            GridSplitter splitter = new GridSplitter();
            splitter.HorizontalAlignment = HorizontalAlignment.Stretch;
            splitter.ResizeDirection = GridResizeDirection.Columns;
            splitter.Background = Brushes.Transparent;

            grid.ColumnDefinitions.Add(definition);
            Grid.SetColumn(splitter, grid.ColumnDefinitions.Count - 1);
            grid.Children.Add(splitter);
        }

        private void insertColumn(Grid grid, TabControl control, TabControl reference, Dock direction)
        {
            List<TabControl> controls = new List<TabControl>();
            foreach (UIElement element in grid.Children)
            {
                if (element is TabControl)
                {
                    controls.Add(element as TabControl);
                }
            }

            int index = controls.IndexOf(reference);
            Trace.Assert(index > -1);
            controls.Insert(Dock.Left == direction ? index : index + 1, control);

            grid.Children.Clear();
            grid.ColumnDefinitions.Clear();
            int count = controls.Count;
            int lastIndex = count - 1;
            double newWidth = (reference.ActualWidth - GRID_SPLITTER_WEIGHT) * .5;
            for (int i = 0; i < count; ++i)
            {
                TabControl item = controls[i];
                Grid.SetColumn(item, i * 2);
                grid.Children.Add(item);
                this.addColumnDefinition(grid, item == control || item == reference ? newWidth : item.ActualWidth);

                if (i < lastIndex)
                {
                    this.addColumnSplitter(grid);
                }
            }
        }

        private void seperateInColumn(TabControl dropTarget, DragEventArgs e, Dock direction)
        {
            TabItem dropItem = e.Data.GetData(typeof(TabItem)) as TabItem;
            if (null == dropItem)
            {
                return;
            }

            TabControl oldTabControl = dropItem.Parent as TabControl;
            Trace.Assert(null != oldTabControl);
            Trace.Assert(null != dropTarget);

            Grid grid = oldTabControl.Parent as Grid;
            Trace.Assert(null != grid);

            oldTabControl.Items.Remove(dropItem);

            TabControl newTabControl = new TabControl();
            newTabControl.Items.Add(dropItem);
            this.addTabControl(newTabControl);

            this.insertColumn(grid, newTabControl, dropTarget, direction);

            //int lastIndex = grid.ColumnDefinitions.Count - 1;
            //if (index < lastIndex)
            //{
            //    int insertIndex = index + 1;
            //    this.addColumnSplitter(grid, insertIndex++);
            //    int otherIndex = this.addColumnItem(grid, dropTarget.ActualWidth, insertIndex);
            //    grid.ColumnDefinitions[index].Width = grid.ColumnDefinitions[otherIndex].Width;
            //    grid.Children.Insert(insertIndex, newTabControl);
            //    Grid.SetColumn(newTabControl, insertIndex);

            //    UIElement[] elements = new UIElement[grid.Children.Count];
            //    grid.Children.CopyTo(elements, 0);
            //    foreach (UIElement element in elements.Skip(insertIndex + 1))
            //    {
            //        int targetIndex = Grid.GetColumn(element) + 2;
            //        Grid.SetColumn(element, targetIndex);
            //        if (element is TabItem)
            //        {
            //            grid.ColumnDefinitions[targetIndex].Width = new GridLength((element as TabItem).ActualWidth, GridUnitType.Star);
            //        }
            //    }
            //}
            //else
            //{
            //    this.addColumnSplitter(grid);
            //    int otherIndex = this.addColumnItem(grid, dropTarget.ActualWidth);
            //    if (Dock.Right == direction)
            //    {
            //        grid.Children.Add(newTabControl);
            //        Grid.SetColumn(newTabControl, otherIndex);
            //    }
            //    else
            //    {
            //        Trace.Assert(Dock.Left == direction);
            //        grid.Children.Remove(dropTarget);
            //        grid.Children.Insert(index, dropItem);
            //        grid.Children.Add(dropTarget);
            //        Grid.SetColumn(dropItem, otherIndex);
            //        Grid.SetColumn(newTabControl, index);
            //    }
            //}
        }

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);

            if (visualAdded is TabItem)
            {
                this.Items.Remove(visualAdded as UIElement);
                mTabControl.Items.Add(visualAdded);
            }
        }
    }
}

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
    public partial class TabPanel : TabControl
    {
        private Grid mSlotRectangle = null;

        public TabPanel()
        {
            InitializeComponent();
        }

        private void checkDragPosition(DragEventArgs e, out double x, out double y, out double offsetX, out double offsetY, out Rect rect, out Dock direction)
        {
            Point position = e.GetPosition(this);
            x = position.X / this.ActualWidth;
            y = position.Y / this.ActualHeight;
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
                double halfWidth = this.ActualWidth * .5;
                if (x <= .25)
                {
                    direction = Dock.Left;
                    rect = new Rect(0, 0, halfWidth, this.ActualHeight);
                }
                else
                {
                    direction = Dock.Right;
                    rect = new Rect(halfWidth, 0, halfWidth, this.ActualHeight);
                }
            }
            else
            {
                double halfHeight = this.ActualHeight * .5;
                if (y <= .25)
                {
                    direction = Dock.Top;
                    rect = new Rect(0, 0, this.ActualWidth, halfHeight);
                }
                else
                {
                    direction = Dock.Bottom;
                    rect = new Rect(0, halfHeight, this.ActualWidth, halfHeight);
                }
            }
            rect.Offset(this.VisualOffset);
        }

        private void fixParent()
        {
            if (this.Parent is Window)
            {
                Window parent = this.Parent as Window;
                parent.RemoveChild(this);

                Grid root = new Grid();
                //root.Background = Brushes.YellowGreen;
                parent.Content = root;

                root.Children.Add(this);
            }
        }

        private void createSlotGrid()
        {
            if (null != mSlotRectangle)
            {
                return;
            }

            this.fixParent();
            Trace.Assert(this.Parent is Grid);

            Grid grid = this.Parent as Grid;
            mSlotRectangle = new Grid();
            mSlotRectangle.Visibility = Visibility.Hidden;
            mSlotRectangle.VerticalAlignment = VerticalAlignment.Top;
            mSlotRectangle.HorizontalAlignment = HorizontalAlignment.Left;
            mSlotRectangle.Background = Brushes.Black;
            mSlotRectangle.Opacity = .5;
            mSlotRectangle.IsHitTestVisible = false;
            grid.Children.Add(mSlotRectangle);
        }

        private void destroySlotGrid()
        {
            if (null == mSlotRectangle)
            {
                return;
            }

            mSlotRectangle.Parent.RemoveChild(mSlotRectangle);
            mSlotRectangle = null;
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            base.OnDragEnter(e);

                Trace.WriteLine($"Enter {this.Items.Count}");
            this.createSlotGrid();
        }

        protected override void OnDragLeave(DragEventArgs e)
        {
            base.OnDragLeave(e);

                Trace.WriteLine($"Leave {this.Items.Count}");
            this.destroySlotGrid();
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            base.OnDragOver(e);

            Rect rect;
            Dock direction;
            double x, y, offsetX, offsetY;
            this.checkDragPosition(e, out x, out y, out offsetX, out offsetY, out rect, out direction);

            if (offsetX > .25 && offsetY > .25)
            {
                mSlotRectangle.Visibility = Visibility.Hidden;
                return;
            }

            mSlotRectangle.Visibility = Visibility.Visible;
            mSlotRectangle.Width = rect.Width;
            mSlotRectangle.Height = rect.Height;
            mSlotRectangle.Margin = new Thickness(rect.X, rect.Y, 0, 0);
        }

        protected override void OnDrop(DragEventArgs e)
        {
            base.OnDrop(e);
            this.destroySlotGrid();

            Rect rect;
            Dock direction;
            double x, y, offsetX, offsetY;
            this.checkDragPosition(e, out x, out y, out offsetX, out offsetY, out rect, out direction);

            if (offsetX > .25 && offsetY > .25)
            {
                this.createNewWindow(e);
                return;
            }

            switch (direction)
            {
                case Dock.Left:
                case Dock.Right:
                    this.seperateHorizontally(e, direction);
                    break;
            }
        }

        private void createNewWindow(DragEventArgs e)
        {
            TabItem item = e.Data.GetData(typeof(TabItem)) as TabItem;
            if (null == item)
            {
                return;
            }

            TabPanel parent = item.Parent as TabPanel;
            Trace.Assert(this == parent);
            parent.Items.Remove(item);

            Window window = new Window();
            window.Width = parent.ActualWidth + window.BorderThickness.Left + window.BorderThickness.Right;
            window.Height = parent.ActualHeight + window.BorderThickness.Top + window.BorderThickness.Bottom + 30; // Height of title bar = 30

            parent = new TabPanel();
            window.Content = parent;

            parent.Items.Add(item);

            Point mousePosition = this.PointToScreen(e.GetPosition(this));
            window.Left = mousePosition.X;
            window.Top = mousePosition.Y;
            window.Show();
        }

        private int addColumnItem(Grid grid, double parentWidth, int insertIndex = -1)
        {
            ColumnDefinition definition = new ColumnDefinition();
            definition.Width = new GridLength((parentWidth - 2) * .5, GridUnitType.Star);
            if (insertIndex >= 0)
            {
                grid.ColumnDefinitions.Insert(insertIndex, definition);
                return insertIndex;
            }
            grid.ColumnDefinitions.Add(definition);
            return grid.ColumnDefinitions.Count - 1;
        }

        private void addColumnSplitter(Grid grid, int insertIndex = -1)
        {
            ColumnDefinition definition = new ColumnDefinition();
            definition.Width = new GridLength(2, GridUnitType.Pixel);

            GridSplitter splitter = new GridSplitter();
            splitter.HorizontalAlignment = HorizontalAlignment.Stretch;
            splitter.ResizeDirection = GridResizeDirection.Columns;
            splitter.Background = Brushes.Black;

            if (insertIndex >= 0)
            {
                grid.ColumnDefinitions.Insert(insertIndex, definition);
                Grid.SetColumn(splitter, insertIndex);
                (this.Parent as Grid).Children.Insert(insertIndex, splitter);
            }
            else
            {
                grid.ColumnDefinitions.Add(definition);
                Grid.SetColumn(splitter, grid.ColumnDefinitions.Count - 1);
                (this.Parent as Grid).Children.Add(splitter);
            }
        }

        private void seperateHorizontally(DragEventArgs e, Dock direction)
        {
            TabItem item = e.Data.GetData(typeof(TabItem)) as TabItem;
            if (null == item)
            {
                return;
            }

            this.fixParent();

            Grid grid = this.Parent as Grid;
            Trace.Assert(null != grid);

            Trace.Assert(this == item.Parent);
            this.Items.Remove(item);

            TabPanel other = new TabPanel();
            other.Items.Add(item);

            int index = Grid.GetColumn(this);
            if (0 == index && 0 == grid.ColumnDefinitions.Count)
            {
                this.addColumnItem(grid, this.ActualWidth);
            }

            int lastIndex = grid.ColumnDefinitions.Count - 1;
            if (index < lastIndex)
            {
                int insertIndex = index + 1;
                this.addColumnSplitter(grid, insertIndex++);
                int otherIndex = this.addColumnItem(grid, this.ActualWidth, insertIndex);
                grid.ColumnDefinitions[index].Width = grid.ColumnDefinitions[otherIndex].Width;
                grid.Children.Insert(insertIndex, other);
                Grid.SetColumn(other, insertIndex);

                UIElement[] elements = new UIElement[grid.Children.Count];
                grid.Children.CopyTo(elements, 0);
                foreach (UIElement element in elements.Skip(insertIndex+1))
                {
                    int targetIndex = Grid.GetColumn(element) + 2;
                    Grid.SetColumn(element, targetIndex);
                    if (element is TabItem)
                    {
                        grid.ColumnDefinitions[targetIndex].Width = new GridLength((element as TabItem).ActualWidth, GridUnitType.Star);
                    }
                }
            }
            else
            {
                this.addColumnSplitter(grid);
                int otherIndex = this.addColumnItem(grid, this.ActualWidth);
                if (Dock.Right == direction)
                {
                    grid.Children.Add(other);
                    Grid.SetColumn(other, otherIndex);
                }
                else
                {
                    Trace.Assert(Dock.Left == direction);
                    grid.Children.Add(other);
                    Grid.SetColumn(item, otherIndex);
                    Grid.SetColumn(other, index);
                }
            }
        }

        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnItemsChanged(e);

            if (NotifyCollectionChangedAction.Remove == e.Action && 0 == this.Items.Count)
            {
                Window window = this.Parent as Window;
                if (typeof(Window) == window?.GetType())
                {
                    window.Close();
                }
            }
        }
    }
}

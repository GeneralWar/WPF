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

        private HashSet<TabControl> mTabControls = new HashSet<TabControl>();

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
            control.onItemsChanged += onTabControlItemsChanged;
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

            if (offsetX > .25 && offsetY > .25)
            {
                this.createNewWindow(e);
                return;
            }

            switch (direction)
            {
                case Dock.Left:
                case Dock.Right:
                    this.seperateInColumn(sender as TabControl, e, direction);
                    break;
                case Dock.Top:
                case Dock.Bottom:
                    this.seperateInRow(sender as TabControl, e, direction);
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
            oldTabControl.Items.Remove(dropItem);

            TabControl newTabControl = new TabControl();
            newTabControl.Items.Add(dropItem);
            this.addTabControl(newTabControl);

            Grid grid = dropTarget.Parent as Grid;
            Trace.Assert(null != grid);
            this.insertColumn(grid, newTabControl, dropTarget, direction);
        }

        private void addRowDefinition(Grid grid, double width)
        {
            RowDefinition definition = new RowDefinition();
            definition.Height = new GridLength(width, GridUnitType.Star);
            grid.RowDefinitions.Add(definition);
        }

        private void addRowSplitter(Grid grid)
        {
            RowDefinition definition = new RowDefinition();
            definition.Height = new GridLength(GRID_SPLITTER_WEIGHT, GridUnitType.Pixel);

            GridSplitter splitter = new GridSplitter();
            splitter.VerticalAlignment = VerticalAlignment.Stretch;
            splitter.Background = Brushes.Transparent;

            grid.RowDefinitions.Add(definition);
            Grid.SetRow(splitter, grid.RowDefinitions.Count - 1);
            grid.Children.Add(splitter);
        }

        private void insertRow(Grid grid, TabControl control, TabControl reference, Dock direction)
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
            controls.Insert(Dock.Top == direction ? index : index + 1, control);

            grid.Children.Clear();
            grid.RowDefinitions.Clear();
            int count = controls.Count;
            int lastIndex = count - 1;
            double newWidth = (reference.ActualWidth - GRID_SPLITTER_WEIGHT) * .5;
            for (int i = 0; i < count; ++i)
            {
                TabControl item = controls[i];
                Grid.SetRow(item, i * 2);
                grid.Children.Add(item);
                this.addRowDefinition(grid, item == control || item == reference ? newWidth : item.ActualWidth);

                if (i < lastIndex)
                {
                    this.addRowSplitter(grid);
                }
            }
        }

        private void seperateInRow(TabControl dropTarget, DragEventArgs e, Dock direction)
        {
            TabItem dropItem = e.Data.GetData(typeof(TabItem)) as TabItem;
            if (null == dropItem)
            {
                return;
            }

            TabControl oldTabControl = dropItem.Parent as TabControl;
            Trace.Assert(null != oldTabControl);
            Trace.Assert(null != dropTarget);
            oldTabControl.Items.Remove(dropItem);

            TabControl newTabControl = new TabControl();
            newTabControl.Items.Add(dropItem);
            this.addTabControl(newTabControl);


            Grid targetGrid = dropTarget.Parent as Grid; 
            Trace.Assert(null != targetGrid);
            this.insertRow(targetGrid, newTabControl, dropTarget, direction);

            UIElement[] children = new UIElement[targetGrid.Children.Count];
            targetGrid.Children.CopyTo(children, 0);
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

        private void removeTabControl(Grid grid, TabControl control)
        {
            Trace.Assert(0 == control.Items.Count);

            List<TabControl> controls = new List<TabControl>();
            foreach (UIElement element in grid.Children)
            {
                TabControl tab = element as TabControl;
                if (tab?.Items.Count > 0)
                {
                    controls.Add(tab);
                }
            }

            grid.Children.Clear();
            controls.Remove(control);
            if (0 == controls.Count)
            {
                return;
            }

            int count = controls.Count;
            int lastIndex = count - 1;
            if (grid.ColumnDefinitions.Count > 0)
            {
                grid.ColumnDefinitions.Clear();
                for (int i = 0; i < count; ++i)
                {
                    TabControl item = controls[i];
                    Grid.SetColumn(item, i * 2);
                    grid.Children.Add(item);
                    this.addColumnDefinition(grid, item.ActualWidth);

                    if (i < lastIndex)
                    {
                        this.addColumnSplitter(grid);
                    }
                }
            }
            else 
            {
                Trace.Assert(grid.RowDefinitions.Count > 0);
                grid.RowDefinitions.Clear();
                for (int i = 0; i < count; ++i)
                {
                    TabControl item = controls[i];
                    Grid.SetColumn(item, i * 2);
                    grid.Children.Add(item);
                    this.addRowDefinition(grid, item.ActualWidth);

                    if (i < lastIndex)
                    {
                        this.addRowSplitter(grid);
                    }
                }
            }
        }

        private void onTabControlItemsChanged(TabControl control, NotifyCollectionChangedEventArgs e)
        {
            if (NotifyCollectionChangedAction.Remove == e.Action && 0 == control.Items.Count)
            {
                Trace.Assert(mPanelGrid == control.Parent);
                this.removeTabControl(mPanelGrid, control);
                if (mPanelGrid.Children.Count > 0)
                {
                    return;
                }

                Window window = mPanelGrid.GetTopWindow();
                if (window?.GetType() == typeof(Window))
                {
                    window.Close();
                }
            }
        }
    }
}

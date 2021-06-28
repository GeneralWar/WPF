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
            Point offset = mRootGrid.PointFromScreen(control.PointToScreen(new Point()));
            rect.Offset(offset.X, offset.Y);
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

            this.seperate(sender as TabControl, e, direction);
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
            splitter.VerticalAlignment = VerticalAlignment.Stretch;
            splitter.ResizeDirection = GridResizeDirection.Columns;
            splitter.Background = Brushes.Transparent;

            grid.ColumnDefinitions.Add(definition);
            Grid.SetColumn(splitter, grid.ColumnDefinitions.Count - 1);
            grid.Children.Add(splitter);
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
            splitter.HorizontalAlignment = HorizontalAlignment.Stretch;
            splitter.VerticalAlignment = VerticalAlignment.Stretch;
            splitter.ResizeDirection = GridResizeDirection.Rows;
            splitter.Background = Brushes.Transparent;

            grid.RowDefinitions.Add(definition);
            Grid.SetRow(splitter, grid.RowDefinitions.Count - 1);
            grid.Children.Add(splitter);
        }

        private Grid replaceTabControlWithGrid(TabControl control)
        {
            Grid parent = control.Parent as Grid;
            Trace.Assert(null != parent);

            int row = Grid.GetRow(control);
            int column = Grid.GetColumn(control);
            int index = parent.Children.IndexOf(control);
            Trace.Assert(index > -1);
            parent.Children.RemoveAt(index);

            Grid grid = new Grid();
            Grid.SetRow(grid, row);
            Grid.SetColumn(grid, column);
            parent.Children.Insert(index, grid);
            grid.Children.Add(control);
            return grid;
        }

        private void groupGrid(Grid grid, List<FrameworkElement> controls, Action<UIElement, int> SetIndex, Action<Grid, double> AddDefinition, Action<Grid> AddSplitter, Func<FrameworkElement, double> GetItemLength)
        {
            int count = controls.Count;
            int lastIndex = count - 1;
            for (int i = 0; i < count; ++i)
            {
                FrameworkElement item = controls[i];
                SetIndex(item, i * 2);
                grid.Children.Add(item);
                AddDefinition(grid, GetItemLength(item));

                if (i < lastIndex)
                {
                    AddSplitter(grid);
                }
            }
        }

        private void insertTabPanel(TabControl control, TabControl reference, Dock direction)
        {
            Grid grid = reference.Parent as Grid;
            Trace.Assert(null != grid);

            List<FrameworkElement> controls = new List<FrameworkElement>();
            foreach (UIElement element in grid.Children)
            {
                if (element is not GridSplitter)
                {
                    controls.Add(element as FrameworkElement);
                }
            }

            double newLength;
            int index = controls.IndexOf(reference);
            Trace.Assert(index > -1);
            if (Dock.Left == direction || Dock.Right == direction)
            {
                if (grid.RowDefinitions.Count > 0)
                {
                    this.replaceTabControlWithGrid(reference);
                    this.insertTabPanel(control, reference, direction);
                    return;
                }

                grid.Children.Clear();
                grid.ColumnDefinitions.Clear();
                newLength = (reference.ActualWidth - GRID_SPLITTER_WEIGHT) * .5;
                controls.Insert(Dock.Left == direction ? index : index + 1, control);
                this.groupGrid(grid, controls, Grid.SetColumn, this.addColumnDefinition, this.addColumnSplitter, item => item == control || item == reference ? newLength : item.ActualWidth);
            }
            else
            {
                Trace.Assert(Dock.Top == direction || Dock.Bottom == direction);

                if (grid.ColumnDefinitions.Count > 0)
                {
                    this.replaceTabControlWithGrid(reference);
                    this.insertTabPanel(control, reference, direction);
                    return;
                }

                grid.Children.Clear();
                grid.RowDefinitions.Clear();
                newLength = (reference.ActualHeight - GRID_SPLITTER_WEIGHT) * .5;
                controls.Insert(Dock.Top == direction ? index : index + 1, control);
                this.groupGrid(grid, controls, Grid.SetRow, this.addRowDefinition, this.addRowSplitter, item => item == control || item == reference ? newLength : item.ActualHeight);
            }
        }

        private void seperate(TabControl dropTarget, DragEventArgs e, Dock direction)
        {
            TabItem dropItem = e.Data.GetData(typeof(TabItem)) as TabItem;
            if (null == dropItem)
            {
                return;
            }

            if (dropTarget == dropItem.Parent && 1 == dropTarget.Items.Count)
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

            this.insertTabPanel(newTabControl, dropTarget, direction);
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

        private void removeGrid(Grid grid)
        {
            Trace.Assert(0 == grid.Children.Count);

            if (grid == mPanelGrid)
            {
                return;
            }

            this.removeFromParent(grid);
        }

        private void removeFromParent(FrameworkElement control)
        {
            Grid parent = control.Parent as Grid;
            Trace.Assert(null != parent);

            List<FrameworkElement> controls = new List<FrameworkElement>();
            foreach (UIElement element in parent.Children)
            {
                TabControl tab = element as TabControl;
                if (tab?.Items.Count > 0)
                {
                    controls.Add(tab);
                    continue;
                }
                Grid panel = element as Grid;
                if (panel?.Children.Count > 0)
                {
                    controls.Add(panel);
                    continue;
                }
            }

            parent.Children.Clear();
            controls.Remove(control);
            if (0 == controls.Count)
            {
                this.removeGrid(parent);
                return;
            }

            if (parent.ColumnDefinitions.Count > 0)
            {
                parent.ColumnDefinitions.Clear();
                this.groupGrid(parent, controls, Grid.SetColumn, this.addColumnDefinition, this.addColumnSplitter, item => item.ActualWidth);
            }
            else
            {
                Trace.Assert(parent.RowDefinitions.Count > 0);

                parent.RowDefinitions.Clear();
                this.groupGrid(parent, controls, Grid.SetRow, this.addRowDefinition, this.addRowSplitter, item => item.ActualHeight);
            }
        }

        private void removeTabControl(TabControl control)
        {
            Trace.Assert(0 == control.Items.Count);
            this.removeFromParent(control);
        }

        private void onTabControlItemsChanged(TabControl control, NotifyCollectionChangedEventArgs e)
        {
            if (NotifyCollectionChangedAction.Remove == e.Action && 0 == control.Items.Count)
            {
                Grid grid = control.Parent as Grid;
                Trace.Assert(null != grid);
                this.removeTabControl(control);
                if (grid.Children.Count > 0)
                {
                    return;
                }

                Window window = grid.GetTopWindow();
                if (window?.GetType() == typeof(Window))
                {
                    window.Close();
                }
            }
        }

        public string SaveLayout()
        {
            return "";
        }

        public void LoadLayout(string layout)
        {

        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;

namespace General.WPF
{
    /// <summary>
    /// TabPanel.xaml 的交互逻辑
    /// </summary>
    public partial class TabPanel : UserControl
    {
        public const double GRID_SPLITTER_WEIGHT = 2;
        public const double TAB_MASK_WIDTH = 6;

        static public readonly DependencyProperty ItemsProperty = DependencyProperty.Register("ItemsProperty", typeof(UIElementCollection), typeof(TabPanel), new PropertyMetadata(null));
        /// <summary>
        /// Gets or sets items for the TabPanel
        /// </summary>
        public UIElementCollection Items { get { return GetValue(ItemsProperty) as UIElementCollection; } set { SetValue(ItemsProperty, value); } }

        private HashSet<TabControl> mTabControls = new HashSet<TabControl>();

        public bool DragEnabled { get; private set; } = true;

        public TabPanel()
        {
            InitializeComponent();
            this.Items = new UIElementCollection(this, this);
            this.addTabControl(mTabControl);
        }

        public void SetDragEnable(bool enabled)
        {
            this.DragEnabled = enabled;
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

        private void checkDragPosition(TabControl control, DragEventArgs e, out double x, out double y, out double offsetX, out double offsetY, out Rect rect, out Dock direction, out int tabIndex)
        {
            Trace.Assert(control.Items.Count > 0);

            Point position = e.GetPosition(control);
            x = position.X / control.ActualWidth;
            y = position.Y / control.ActualHeight;
            offsetX = Math.Min(x, 1.0 - x);
            offsetY = Math.Min(y, 1.0 - y);
            direction = (Dock)(-1);
            tabIndex = -1;

            if (offsetX > .25 && offsetY > .25)
            {
                rect = new Rect();
                return;
            }

            //IInputElement hit = control.InputHitTest(position);
            //if (hit is TextBlock)
            //{
            //    tabIndex = control.Items.IndexOf(hit);
            //    if(tabIndex > -1)
            //    {
            //        return;
            //    }
            //}
            int itemCount = control.Items.Count;
            Trace.Assert(itemCount > 0);
            for (int i = 0; i < itemCount; ++i)
            {
                FrameworkElement item = control.Items[i] as FrameworkElement;
                if (null == item)
                {
                    continue;
                }

                IInputElement hit = item.InputHitTest(e.GetPosition(item));
                if (null == hit)
                {
                    continue;
                }

                Point start = control.PointFromScreen(item.PointToScreen(new Point()));
                rect = new Rect(start.X, start.Y, TAB_MASK_WIDTH, item.ActualHeight);
                tabIndex = i;
                goto OFFSET;
            }

            FrameworkElement lastTab = control.Items[itemCount - 1] as FrameworkElement;
            if (position.Y < lastTab.ActualHeight)
            {
                Point start = control.PointFromScreen(lastTab.PointToScreen(new Point(lastTab.ActualWidth, 0)));
                rect = new Rect(start.X, start.Y, TAB_MASK_WIDTH, lastTab.ActualHeight);
                tabIndex = itemCount;
                goto OFFSET;
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

        OFFSET:
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
            TabItem item = e.Data.GetData(typeof(TabItem)) as TabItem;
            if (null == item)
            {
                return;
            }

            Rect rect;
            int tabIndex;
            Dock direction;
            double x, y, offsetX, offsetY;
            this.checkDragPosition(sender as TabControl, e, out x, out y, out offsetX, out offsetY, out rect, out direction, out tabIndex);

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
            TabItem item = e.Data.GetData(typeof(TabItem)) as TabItem;
            if (null == item)
            {
                return;
            }

            this.hideSlotMask();

            Rect rect;
            int tabIndex;
            Dock direction;
            double x, y, offsetX, offsetY;
            this.checkDragPosition(sender as TabControl, e, out x, out y, out offsetX, out offsetY, out rect, out direction, out tabIndex);

            if (tabIndex > -1)
            {
                Trace.Assert(null != item);

                TabControl parent = item.Parent as TabControl;
                Trace.Assert(null != parent);

                int index = parent.Items.IndexOf(item);
                Trace.Assert(index > -1);

                TabControl control = sender as TabControl;
                if (control == parent && (tabIndex == index || tabIndex - 1 == index))
                {
                    return;
                }

                parent.Items.RemoveAt(index);
                if (tabIndex < control.Items.Count)
                {
                    control.Items.Insert(tabIndex, item);
                }
                else
                {
                    control.Items.Add(item);
                }
                return;
            }

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

        private void groupGrid(Grid source, Grid target)
        {
            if (source.ColumnDefinitions.Count > 0)
            {
                foreach (ColumnDefinition definition in source.ColumnDefinitions)
                {
                    target.ColumnDefinitions.Add(definition);
                }
            }
            else
            {
                Trace.Assert(source.RowDefinitions.Count > 0);
                foreach (RowDefinition definition in source.RowDefinitions)
                {
                    target.RowDefinitions.Add(definition);
                }
            }
        }

        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);

            if (visualAdded is TabItem)
            {
                this.Items.Remove(visualAdded as UIElement);
                mTabControl.Items.Add(visualAdded);
            }
            else if (visualAdded is Grid)
            {
                //mPanelGrid.Children.Clear();
                //mPanelGrid.RowDefinitions.Clear();
                //mPanelGrid.ColumnDefinitions.Clear();
                //this.groupGrid(visualAdded as Grid, mPanelGrid);
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

        private const string ATTRIBUTE_NAME = "Name";
        private const string ATTRIBUTE_TYPE = "Type";
        private const string ATTRIBUTE_SIZE = "Size";
        private const string ATTRIBUTE_HEADER = "Header";
        private const string ATTRIBUTE_ASSEMBLY = "Assembly";
        private const string ATTRIBUTE_DIRECTION = "Direction";
        private const string ATTRIBUTE_CONTENT_TYPE = "ContentType";
        private const string ATTRIBUTE_CONTENT_STRING = "ContentString";
        private const string ATTRIBUTE_CONTENT_ASSEMBLY = "ContentAssembly";

        private const string NAME_ROOT = "Root";
        private const string NAME_GRID = "Grid";
        private const string NAME_ITEM = "Item";
        private const string NAME_TAB = "Tab";

        private void saveLayout(XmlElement element, FrameworkElement item)
        {
            element.SetAttribute(ATTRIBUTE_NAME, item.Name);
            element.SetAttribute(ATTRIBUTE_SIZE, $"{(int)Math.Round(item.ActualWidth)},{(int)Math.Round(item.ActualHeight)}");

            Type type = item.GetType();
            element.SetAttribute(ATTRIBUTE_TYPE, type.FullName);
            element.SetAttribute(ATTRIBUTE_ASSEMBLY, type.Assembly.GetName().Name);
        }

        private void saveLayout(XmlElement element, TabControl control)
        {
            this.saveLayout(element, control as FrameworkElement);
            foreach (UIElement child in control.Items)
            {
                XmlElement e = element.OwnerDocument.CreateElement(NAME_ITEM);
                this.saveLayout(e, child as FrameworkElement);
                if (child is TabItem)
                {
                    TabItem item = child as TabItem;
                    e.SetAttribute(ATTRIBUTE_HEADER, item.Header.ToString());
                    if (null != item.Content)
                    {
                        e.SetAttribute(ATTRIBUTE_CONTENT_STRING, item.Content.ToString());

                        Type type = item.Content.GetType();
                        e.SetAttribute(ATTRIBUTE_CONTENT_TYPE, type.FullName);
                        e.SetAttribute(ATTRIBUTE_CONTENT_ASSEMBLY, type.Assembly.GetName().Name);
                    }
                }
                element.AppendChild(e);
            }
        }

        private void saveLayout(XmlElement element, Grid grid)
        {
            this.saveLayout(element, grid as FrameworkElement);
            element.SetAttribute(ATTRIBUTE_DIRECTION, (grid.RowDefinitions.Count > 0 ? GridResizeDirection.Rows : GridResizeDirection.Columns).ToString());
            foreach (UIElement child in grid.Children)
            {
                if (child is GridSplitter)
                {
                    continue;
                }

                Trace.Assert(child is FrameworkElement);

                XmlElement e;
                if (child is Grid)
                {
                    this.saveLayout(e = element.OwnerDocument.CreateElement(NAME_GRID), child as Grid);
                }
                else
                {
                    Trace.Assert(child is TabControl);
                    this.saveLayout(e = element.OwnerDocument.CreateElement(NAME_TAB), child as TabControl);
                }
                element.AppendChild(e);
            }
        }

        public string SaveLayout()
        {
            XmlDocument document = new XmlDocument();
            XmlElement root = document.CreateElement(NAME_ROOT);
            document.AppendChild(root);

            this.saveLayout(root, mPanelGrid);

            using (MemoryStream stream = new MemoryStream())
            {
                using (TextWriter writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    document.Save(stream);
                    return Encoding.UTF8.GetString(stream.ToArray());
                }
            }
        }

        private FrameworkElement loadLayout(XmlElement element, FrameworkElement item = null)
        {
            if (element.HasAttribute(ATTRIBUTE_ASSEMBLY) && element.HasAttribute(ATTRIBUTE_TYPE))
            {
                string typename = element.GetAttribute(ATTRIBUTE_TYPE);
                if (null == item)
                {
                    string assemblyName = element.GetAttribute(ATTRIBUTE_ASSEMBLY);
                    Assembly assembly = Assembly.Load(assemblyName);
                    Type type = assembly.GetType(typename);
                    item = Activator.CreateInstance(type) as FrameworkElement;
                }
                Trace.Assert(item.GetType().FullName == typename);
            }
            if (element.HasAttribute(ATTRIBUTE_NAME))
            {
                item.Name = element.GetAttribute(ATTRIBUTE_NAME);
            }
            if (element.HasAttribute(ATTRIBUTE_SIZE))
            {
                string sizeContent = element.GetAttribute(ATTRIBUTE_SIZE);
                if (!string.IsNullOrWhiteSpace(sizeContent))
                {
                    string[] parts = sizeContent.Split(",");
                    if (2 == parts.Length)
                    {
                        item.Width = double.Parse(parts[0].Trim());
                        item.Height = double.Parse(parts[1].Trim());
                    }
                }
            }
            return item;
        }

        private void loadLayout(XmlElement element, TabControl control)
        {
            this.saveLayout(element, control as FrameworkElement);
            foreach (XmlElement child in element.ChildNodes)
            {
                TabItem item = this.loadLayout(child) as TabItem;
                Trace.Assert(null != item);

                item.Header = child.GetAttribute(ATTRIBUTE_HEADER);

                string assemblyName = child.GetAttribute(ATTRIBUTE_CONTENT_ASSEMBLY);
                Assembly assembly = Assembly.Load(assemblyName);
                string contentTypeName = child.GetAttribute(ATTRIBUTE_CONTENT_TYPE);
                Type contentType = assembly.GetType(contentTypeName);
                Trace.Assert(null != contentType);
                if (typeof(string) == contentType)
                {
                    item.Content = child.GetAttribute(ATTRIBUTE_CONTENT_STRING);
                }
                else
                {
                    item.Content = Activator.CreateInstance(contentType);
                }
                control.Items.Add(item);
            }
            this.addTabControl(control);
        }

        private void loadLayout(XmlElement element, Grid grid)
        {
            this.loadLayout(element, grid as FrameworkElement);

            Trace.Assert(element.HasAttribute(ATTRIBUTE_DIRECTION));

            GridResizeDirection direction = Enum.Parse<GridResizeDirection>(element.GetAttribute(ATTRIBUTE_DIRECTION), true);
            List<FrameworkElement> controls = new List<FrameworkElement>();
            foreach (XmlElement child in element.ChildNodes)
            {
                FrameworkElement control = this.loadLayout(child);
                if (control is TabControl)
                {
                    this.loadLayout(child, control as TabControl);
                }
                else if (control is Grid)
                {
                    this.loadLayout(child, control as Grid);
                }
                controls.Add(control);
            }
            if (GridResizeDirection.Rows == direction)
            {
                this.groupGrid(grid, controls, Grid.SetRow, this.addRowDefinition, this.addRowSplitter, item => item.Height);
            }
            else
            {
                Trace.Assert(GridResizeDirection.Columns == direction);
                this.groupGrid(grid, controls, Grid.SetColumn, this.addColumnDefinition, this.addColumnSplitter, item => item.Width);
            }
            controls.ForEach(c => c.Width = c.Height = double.NaN);
        }

        public void LoadLayout(string layout)
        {
            XmlDocument document = new XmlDocument();
            document.LoadXml(layout);

            XmlElement root = document.FirstChild as XmlElement;
            if (null == root || NAME_ROOT != root.Name)
            {
                return;
            }

            mPanelGrid.Children.Clear();
            mPanelGrid.RowDefinitions.Clear();
            mPanelGrid.ColumnDefinitions.Clear();
            this.loadLayout(root, mPanelGrid);
            mPanelGrid.Width = mPanelGrid.Height = double.NaN;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace General.WPF
{
    public class GridSplitter : System.Windows.Controls.GridSplitter
    {
        private class ValueRecord
        {
            public object? binding;
            public double value;
        }

        private Dictionary<DefinitionBase, ValueRecord> mMaxValueRecords = new Dictionary<DefinitionBase, ValueRecord>();

        protected override void OnDraggingChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnDraggingChanged(e);

            if ((bool)e.NewValue)
            {
                this.onDragBegin();
            }
            else
            {
                this.onDragEnd();
            }
        }

        private void onDragBegin()
        {
            Grid? grid = this.FindAncestor<Grid>();
            if (grid is null)
            {
                return;
            }

            this.recordRowValues(grid);
            this.recordColumnValues(grid);
        }

        private void recordValues<DefinitionType>(Grid grid, ICollection<DefinitionType> definitions, Func<UIElement, int> indexChecker, Func<DefinitionType, ValueRecord> recordChecker, Func<DefinitionType, DefinitionType, double> totalLengthChecker, Action<double, DefinitionType, DefinitionType> previousValueProcessor, Action<double, DefinitionType, DefinitionType> nextValueProcessor) where DefinitionType : DefinitionBase
        {
            int index = indexChecker.Invoke(this);
            if (index <= 0)
            {
                return;
            }

            DefinitionType previousDefinition = definitions.ElementAt(index - 1);
            mMaxValueRecords.Add(previousDefinition, recordChecker.Invoke(previousDefinition));

            DefinitionType nextDefinition = definitions.ElementAt(index + 1);
            mMaxValueRecords.Add(nextDefinition, recordChecker.Invoke(nextDefinition));

            double totalLength = totalLengthChecker.Invoke(previousDefinition, nextDefinition);
            previousValueProcessor.Invoke(totalLength, previousDefinition, nextDefinition);
            nextValueProcessor.Invoke(totalLength, previousDefinition, nextDefinition);
        }

        private void recordRowValues(Grid grid)
        {
            this.recordValues(grid, grid.RowDefinitions, Grid.GetRow,
                d => new ValueRecord { binding = d.BindingGroup?.GetValue(RowDefinition.MaxHeightProperty), value = d.MaxHeight },
                (p, n) => p.ActualHeight + this.ActualHeight + n.ActualHeight,
                (t, p, n) => p.MaxHeight = t - this.ActualHeight - n.MinHeight,
                (t, p, n) => n.MaxHeight = t - p.MinHeight - this.ActualHeight);
        }

        private void recordColumnValues(Grid grid)
        {
            this.recordValues(grid, grid.ColumnDefinitions, Grid.GetColumn,
                d => new ValueRecord { binding = d.BindingGroup?.GetValue(ColumnDefinition.MaxWidthProperty), value = d.MaxWidth },
                (p, n) => p.ActualWidth + this.ActualWidth + n.ActualWidth,
                (t, p, n) => p.MaxWidth = t - this.ActualWidth - n.MinWidth,
                (t, p, n) => n.MaxWidth = t - p.MinWidth - this.ActualWidth);
        }

        private void onDragEnd()
        {
            Grid? grid = this.FindAncestor<Grid>();
            if (grid is null)
            {
                return;
            }

            this.restoreRowValues(grid);
            this.restoreColumnValues(grid);
        }

        private void restoreValues<DefinitionType>(Grid grid, ICollection<DefinitionType> definitions, Func<UIElement, int> indexChecker, Action<DefinitionType, ValueRecord> recordProcessor) where DefinitionType : DefinitionBase
        {
            int index = indexChecker.Invoke(this);
            if (index <= 0)
            {
                return;
            }

            ValueRecord? record;
            DefinitionType previousDefinition = definitions.ElementAt(index - 1);
            if (mMaxValueRecords.Remove(previousDefinition, out record))
            {
                recordProcessor.Invoke(previousDefinition, record);
            }

            DefinitionType nextDefinition = definitions.ElementAt(index + 1);
            if (mMaxValueRecords.Remove(nextDefinition, out record))
            {
                recordProcessor.Invoke(nextDefinition, record);
            }
        }

        private void restoreRowValues(Grid grid)
        {
            this.restoreValues(grid, grid.RowDefinitions, Grid.GetRow,
                (d, r) =>
                {
                    if (r.binding is null)
                    {
                        d.MaxHeight = r.value;
                    }
                    else
                    {
                        d.BindingGroup?.SetValue(RowDefinition.MaxHeightProperty, r.binding);
                    }
                });
        }

        private void restoreColumnValues(Grid grid)
        {
            this.restoreValues(grid, grid.ColumnDefinitions, Grid.GetColumn,
                (d, r) =>
                {
                    if (r.binding is null)
                    {
                        d.MaxWidth = r.value;
                    }
                    else
                    {
                        d.BindingGroup?.SetValue(ColumnDefinition.MaxWidthProperty, r.binding);
                    }
                });
        }
    }
}

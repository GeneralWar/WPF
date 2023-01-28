using System;
using System.Collections;
using System.Windows;

namespace General.WPF.Helpers
{
    internal class EnumerateTo
    {
        public DependencyObject From { get; }
        public DependencyObject To { get; }

        private DependencyObject mCurrent;

        /// <summary>
        /// Make sure that 'from' is in front of 'to'
        /// </summary>
        public EnumerateTo(DependencyObject from, DependencyObject to)
        {
            this.From = mCurrent = from;
            this.To = to;
        }

        public void Enumerate(Action<DependencyObject> handler)
        {
            if (this.From == this.To)
            {
                handler.Invoke(this.To);
                return;
            }

            lock (this)
            {
                mCurrent = this.From;

                IEnumerator? enumerator = this.From.GetEnumerator();
                if (enumerator is null)
                {
                    enumerator = GetEnumeratorUpward(this.From);
                }
                if (enumerator is null)
                {
                    throw new InvalidOperationException("理论上应该能找到上一级");
                }

                bool result = this.enumeratorTo(enumerator, this.To, handler);
#if DEBUG
                if (!result)
                {
                    throw new NotImplementedException("理论上应该能在上述步骤中遍历到目标");
                }
#endif
            }
        }

        private bool enumeratorTo(IEnumerator enumerator, DependencyObject to, Action<DependencyObject> handler)
        {
            while (enumerator.MoveNext())
            {
                DependencyObject? current = enumerator.Current as DependencyObject;
                if (current is null)
                {
                    continue;
                }

                handler.Invoke(mCurrent = current);

                if (current == to)
                {
                    return true;
                }

                IEnumerator? childEnumerator = current.GetEnumerator();
                if (childEnumerator is not null && this.enumeratorTo(childEnumerator, to, handler))
                {
                    return true;
                }
            }

            DependencyObject? parent = (mCurrent as FrameworkElement)?.Parent;
            if (parent is not null)
            {
                IEnumerator? nextEnumerator = GetEnumeratorUpward(parent);
                if (nextEnumerator is not null)
                {
                    return this.enumeratorTo(nextEnumerator, to, handler);
                }
            }
            return false;
        }

        static private IEnumerator? GetEnumeratorUpward(DependencyObject target)
        {
            DependencyObject? parent = (target as FrameworkElement)?.Parent;
            IEnumerator? enumerator = parent?.GetEnumerator();
            if (enumerator is not null)
            {
                while (enumerator.MoveNext() && enumerator.Current != target) ;
                return enumerator;
            }

            if (parent is not null)
            {
                enumerator = GetEnumeratorUpward(parent);
            }

            return enumerator;
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;

// Credit goes to the author of UniLinq:
//
// Author:
//   Jb Evain (jbevain@novell.com)
//
// Github:
//   https://github.com/RyotaMurohoshi/UniLinq
//
// Copyright (C) 2007 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
namespace uNature.Wrappers.Linq
{
    public static class LinqWrapper
    {
        enum Fallback
        {
            Default,
            Throw
        }

        public static bool Contains<T>(this T[] arr, T obj)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i].Equals(obj))
                    return true;
            }

            return false;
        }

        public static List<T> ToList<T>(this T[] arr)
        {
            List<T> list = new List<T>(arr.Length);

            for (int i = 0; i < arr.Length; i++)
            {
                list.Add(arr[i]);
            }

            return list;
        }

        #region Select

        public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            Check.SourceAndSelector(source, selector);

            return CreateSelectIterator(source, selector);
        }

        static IEnumerable<TResult> CreateSelectIterator<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, TResult> selector)
        {
            foreach (var element in source)
                yield return selector(element);
        }

        public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, int, TResult> selector)
        {
            Check.SourceAndSelector(source, selector);

            return CreateSelectIterator(source, selector);
        }

        static IEnumerable<TResult> CreateSelectIterator<TSource, TResult>(IEnumerable<TSource> source, Func<TSource, int, TResult> selector)
        {
            int counter = 0;
            foreach (TSource element in source)
            {
                yield return selector(element, counter);
                counter++;
            }
        }

        #endregion

        #region Count

        public static int Count<TSource>(this IEnumerable<TSource> source)
        {
            Check.Source(source);

            var collection = source as ICollection<TSource>;
            if (collection != null)
                return collection.Count;

            int counter = 0;
            using (var enumerator = source.GetEnumerator())
                while (enumerator.MoveNext())
                    checked
                    {
                        counter++;
                    }

            return counter;
        }

        public static int Count<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            Check.SourceAndSelector(source, predicate);

            int counter = 0;
            foreach (var element in source)
                if (predicate(element))
                    checked
                    {
                        counter++;
                    }

            return counter;
        }

        #endregion

        #region First

        static TSource First<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, Fallback fallback)
        {
            foreach (var element in source)
                if (predicate(element))
                    return element;

            if (fallback == Fallback.Throw)
                throw NoMatchingElement();

            return default(TSource);
        }

        public static TSource First<TSource>(this IEnumerable<TSource> source)
        {
            Check.Source(source);

            var list = source as IList<TSource>;
            if (list != null)
            {
                if (list.Count != 0)
                    return list[0];
            }
            else
            {
                using (var enumerator = source.GetEnumerator())
                {
                    if (enumerator.MoveNext())
                        return enumerator.Current;
                }
            }

            throw EmptySequence();
        }

        public static TSource First<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            Check.SourceAndPredicate(source, predicate);

            return source.First(predicate, Fallback.Throw);
        }

        #endregion

        #region FirstOrDefault

        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source)
        {
            Check.Source(source);

            // inline the code to reduce dependency o generic causing AOT errors on device (e.g. bug #3285)
            foreach (var element in source)
                return element;

            return default(TSource);
        }

        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            Check.SourceAndPredicate(source, predicate);

            return source.First(predicate, Fallback.Default);
        }

        #endregion

        #region OrderBy

        public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source,
                Func<TSource, TKey> keySelector)
        {
            return OrderBy<TSource, TKey>(source, keySelector, null);
        }

        public static IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(this IEnumerable<TSource> source,
                Func<TSource, TKey> keySelector,
                IComparer<TKey> comparer)
        {
            Check.SourceAndKeySelector(source, keySelector);

            return new OrderedSequence<TSource, TKey>(source, keySelector, comparer, SortDirection.Ascending);
        }

        #endregion

        #region ToArray

        public static TSource[] ToArray<TSource>(this IEnumerable<TSource> source)
        {
            Check.Source(source);

            TSource[] array;
            var collection = source as ICollection<TSource>;
            if (collection != null)
            {
                if (collection.Count == 0)
                    return new TSource[0];

                array = new TSource[collection.Count];
                collection.CopyTo(array, 0);
                return array;
            }

            int pos = 0;
            array = new TSource[0];
            foreach (var element in source)
            {
                if (pos == array.Length)
                {
                    if (pos == 0)
                        array = new TSource[4];
                    else
                        Array.Resize(ref array, pos * 2);
                }

                array[pos++] = element;
            }

            if (pos != array.Length)
                Array.Resize(ref array, pos);

            return array;
        }

        #endregion

        #region Exception helpers

        static Exception EmptySequence()
        {
            return new InvalidOperationException("Sequence contains no elements");
        }

        static Exception NoMatchingElement()
        {
            return new InvalidOperationException("Sequence contains no matching element");
        }

        static Exception MoreThanOneElement()
        {
            return new InvalidOperationException("Sequence contains more than one element");
        }

        static Exception MoreThanOneMatchingElement()
        {
            return new InvalidOperationException("Sequence contains more than one matching element");
        }

        #endregion
    }

    abstract class OrderedEnumerable<TElement> : IOrderedEnumerable<TElement>
    {
        IEnumerable<TElement> source;

        protected OrderedEnumerable(IEnumerable<TElement> source)
        {
            this.source = source;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public virtual IEnumerator<TElement> GetEnumerator()
        {
            return Sort(source).GetEnumerator();
        }

        public abstract SortContext<TElement> CreateContext(SortContext<TElement> current);

        protected abstract IEnumerable<TElement> Sort(IEnumerable<TElement> source);

        public IOrderedEnumerable<TElement> CreateOrderedEnumerable<TKey>(
            Func<TElement, TKey> selector, IComparer<TKey> comparer, bool descending)
        {
            return new OrderedSequence<TElement, TKey>(this, source, selector, comparer,
                descending ? SortDirection.Descending : SortDirection.Ascending);
        }
    }

    public interface IOrderedEnumerable<TElement> : IEnumerable<TElement>
    {
        IOrderedEnumerable<TElement> CreateOrderedEnumerable<TKey>(Func<TElement, TKey> keySelector, IComparer<TKey> comparer, bool descending);
    }

    static class Check
    {

        public static void Source(object source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
        }

        public static void Source1AndSource2(object source1, object source2)
        {
            if (source1 == null)
                throw new ArgumentNullException("source1");
            if (source2 == null)
                throw new ArgumentNullException("source2");
        }

        public static void SourceAndFuncAndSelector(object source, object func, object selector)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (func == null)
                throw new ArgumentNullException("func");
            if (selector == null)
                throw new ArgumentNullException("selector");
        }

        public static void SourceAndFunc(object source, object func)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (func == null)
                throw new ArgumentNullException("func");
        }

        public static void SourceAndSelector(object source, object selector)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (selector == null)
                throw new ArgumentNullException("selector");
        }

        public static void SourceAndPredicate(object source, object predicate)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (predicate == null)
                throw new ArgumentNullException("predicate");
        }

        public static void FirstAndSecond(object first, object second)
        {
            if (first == null)
                throw new ArgumentNullException("first");
            if (second == null)
                throw new ArgumentNullException("second");
        }

        public static void SourceAndKeySelector(object source, object keySelector)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (keySelector == null)
                throw new ArgumentNullException("keySelector");
        }

        public static void SourceAndKeyElementSelectors(object source, object keySelector, object elementSelector)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (keySelector == null)
                throw new ArgumentNullException("keySelector");
            if (elementSelector == null)
                throw new ArgumentNullException("elementSelector");
        }

        public static void SourceAndKeyResultSelectors(object source, object keySelector, object resultSelector)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (keySelector == null)
                throw new ArgumentNullException("keySelector");
            if (resultSelector == null)
                throw new ArgumentNullException("resultSelector");
        }

        public static void SourceAndCollectionSelectorAndResultSelector(object source, object collectionSelector, object resultSelector)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (collectionSelector == null)
                throw new ArgumentNullException("collectionSelector");
            if (resultSelector == null)
                throw new ArgumentNullException("resultSelector");
        }

        public static void SourceAndCollectionSelectors(object source, object collectionSelector, object selector)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (collectionSelector == null)
                throw new ArgumentNullException("collectionSelector");
            if (selector == null)
                throw new ArgumentNullException("selector");
        }

        public static void JoinSelectors(object outer, object inner, object outerKeySelector, object innerKeySelector, object resultSelector)
        {
            if (outer == null)
                throw new ArgumentNullException("outer");
            if (inner == null)
                throw new ArgumentNullException("inner");
            if (outerKeySelector == null)
                throw new ArgumentNullException("outerKeySelector");
            if (innerKeySelector == null)
                throw new ArgumentNullException("innerKeySelector");
            if (resultSelector == null)
                throw new ArgumentNullException("resultSelector");
        }

        public static void GroupBySelectors(object source, object keySelector, object elementSelector, object resultSelector)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            if (keySelector == null)
                throw new ArgumentNullException("keySelector");
            if (elementSelector == null)
                throw new ArgumentNullException("elementSelector");
            if (resultSelector == null)
                throw new ArgumentNullException("resultSelector");
        }
    }

    enum SortDirection
    {
        Ascending,
        Descending
    }

    class OrderedSequence<TElement, TKey> : OrderedEnumerable<TElement>
    {

        OrderedEnumerable<TElement> parent;
        Func<TElement, TKey> selector;
        IComparer<TKey> comparer;
        SortDirection direction;

        internal OrderedSequence(IEnumerable<TElement> source, Func<TElement, TKey> key_selector, IComparer<TKey> comparer, SortDirection direction)
            : base(source)
        {
            this.selector = key_selector;
            this.comparer = comparer ?? Comparer<TKey>.Default;
            this.direction = direction;
        }

        internal OrderedSequence(OrderedEnumerable<TElement> parent, IEnumerable<TElement> source, Func<TElement, TKey> keySelector, IComparer<TKey> comparer, SortDirection direction)
            : this(source, keySelector, comparer, direction)
        {
            this.parent = parent;
        }

        public override IEnumerator<TElement> GetEnumerator()
        {
            return base.GetEnumerator();
        }

        public override SortContext<TElement> CreateContext(SortContext<TElement> current)
        {
            SortContext<TElement> context = new SortSequenceContext<TElement, TKey>(selector, comparer, direction, current);

            if (parent != null)
                return parent.CreateContext(context);

            return context;
        }

        protected override IEnumerable<TElement> Sort(IEnumerable<TElement> source)
        {
            return QuickSort<TElement>.Sort(source, CreateContext(null));
        }
    }

    abstract class SortContext<TElement> : IComparer<int>
    {

        protected SortDirection direction;
        protected SortContext<TElement> child_context;

        protected SortContext(SortDirection direction, SortContext<TElement> child_context)
        {
            this.direction = direction;
            this.child_context = child_context;
        }

        public abstract void Initialize(TElement[] elements);

        public abstract int Compare(int first_index, int second_index);
    }

    class SortSequenceContext<TElement, TKey> : SortContext<TElement>
    {

        Func<TElement, TKey> selector;
        IComparer<TKey> comparer;
        TKey[] keys;

        public SortSequenceContext(Func<TElement, TKey> selector, IComparer<TKey> comparer, SortDirection direction, SortContext<TElement> child_context)
            : base(direction, child_context)
        {
            this.selector = selector;
            this.comparer = comparer;
        }

        public override void Initialize(TElement[] elements)
        {
            if (child_context != null)
                child_context.Initialize(elements);

            keys = new TKey[elements.Length];
            for (int i = 0; i < keys.Length; i++)
                keys[i] = selector(elements[i]);
        }

        public override int Compare(int first_index, int second_index)
        {
            int comparison = comparer.Compare(keys[first_index], keys[second_index]);

            if (comparison == 0)
            {
                if (child_context != null)
                    return child_context.Compare(first_index, second_index);

                comparison = direction == SortDirection.Descending
                    ? second_index - first_index
                    : first_index - second_index;
            }

            return direction == SortDirection.Descending ? -comparison : comparison;
        }
    }

    class QuickSort<TElement>
    {

        TElement[] elements;
        int[] indexes;
        SortContext<TElement> context;

        QuickSort(IEnumerable<TElement> source, SortContext<TElement> context)
        {
            List<TElement> temp = new List<TElement>();
            foreach (TElement element in source)
            {
                temp.Add(element);
            }

            this.elements = temp.ToArray();
            this.indexes = CreateIndexes(elements.Length);
            this.context = context;
        }

        static int[] CreateIndexes(int length)
        {
            var indexes = new int[length];
            for (int i = 0; i < length; i++)
                indexes[i] = i;

            return indexes;
        }

        void PerformSort()
        {
            // If the source contains just zero or one element, there's no need to sort
            if (elements.Length <= 1)
                return;

            context.Initialize(elements);

            // Then sorts the elements according to the collected
            // key values and the selected ordering
            Array.Sort<int>(indexes, context);
        }

        public static IEnumerable<TElement> Sort(IEnumerable<TElement> source, SortContext<TElement> context)
        {
            var sorter = new QuickSort<TElement>(source, context);

            sorter.PerformSort();

            for (int i = 0; i < sorter.elements.Length; i++)
                yield return sorter.elements[sorter.indexes[i]];
        }
    }
}

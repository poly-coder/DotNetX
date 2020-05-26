using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DotNetX
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Singleton<T>(this T value)
        {
            yield return value;
        }

        public static string Concatenate(this IEnumerable<string> source, string separator = ", ")
        {
            return String.Join(separator, source);
        }

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
            {
                action(item);
            }
        }

        public static IEnumerable<T> StartWith<T>(this IEnumerable<T> source, IEnumerable<T> toPrepend)
        {
            return toPrepend.Concat(source);
        }

        public static IEnumerable<T> StartWith<T>(this IEnumerable<T> source, params T[] toPrepend)
        {
            return toPrepend.Concat(source);
        }


        public static int IndexOf<T>(this IReadOnlyList<T> source, Func<T, bool> predicate)
        {
            for (int i = 0; i < source.Count; i++)
            {
                if (predicate(source[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        public static int IndexOf<T>(this IReadOnlyList<T> source, T item, IEqualityComparer<T> comparer = null)
        {
            comparer = comparer ?? EqualityComparer<T>.Default;
            return source.IndexOf(e => comparer.Equals(e, item));
        }


        public static bool IsEqualTo<T>(this IEnumerable<T> source1, IEnumerable<T> source2, IEqualityComparer<T> comparer = null)
        {
            if ((source1 == null) != (source2 == null)) return false;
            if (source1 == null) return true;

            if (source1 is T[] array1 && source2 is T[] array2)
            {
                return array1.IsEqualTo(array2, comparer);
            }

            if (source1 is IReadOnlyCollection<T> collection1 && source2 is IReadOnlyCollection<T> collection2)
            {
                return collection1.IsEqualTo(collection2, comparer);
            }

            if (source1 is ICollection<T> oldcollection1 && source2 is ICollection<T> oldcollection2)
            {
                return oldcollection1.IsEqualTo(oldcollection2, comparer);
            }

            comparer = comparer ?? EqualityComparer<T>.Default;
            using var enumerator1 = source1.GetEnumerator();
            using var enumerator2 = source2.GetEnumerator();
            while (true)
            {
                var moved1 = enumerator1.MoveNext();
                var moved2 = enumerator2.MoveNext();
                if (moved1 != moved2) return false;
                if (!moved1) return true;
                if (!comparer.Equals(enumerator1.Current, enumerator2.Current)) return false;
            }
        }

        public static bool IsEqualTo<T>(this ICollection<T> source1, ICollection<T> source2, IEqualityComparer<T> comparer = null)
        {
            if ((source1 == null) != (source2 == null)) return false;
            if (source1 == null) return true;

            if (source1 is IReadOnlyCollection<T> collection1 && source2 is IReadOnlyCollection<T> collection2)
            {
                return collection1.IsEqualTo(collection2, comparer);
            }

            if (source1.Count != source2.Count) return false;

            comparer = comparer ?? EqualityComparer<T>.Default;
            using var enumerator1 = source1.GetEnumerator();
            using var enumerator2 = source2.GetEnumerator();
            while (true)
            {
                var moved1 = enumerator1.MoveNext();
                var moved2 = enumerator2.MoveNext();
                if (moved1 != moved2) return false; // Weird, count were equal. This should not happend
                if (!moved1) return true;
                if (!comparer.Equals(enumerator1.Current, enumerator2.Current)) return false;
            }
        }

        public static bool IsEqualTo<T>(this IReadOnlyCollection<T> source1, IReadOnlyCollection<T> source2, IEqualityComparer<T> comparer = null)
        {
            if ((source1 == null) != (source2 == null)) return false;
            if (source1 == null) return true;

            if (source1 is IReadOnlyList<T> list1 && source2 is IReadOnlyList<T> list2)
            {
                return list1.IsEqualTo(list2, comparer);
            }

            if (source1.Count != source2.Count) return false;

            comparer = comparer ?? EqualityComparer<T>.Default;
            using var enumerator1 = source1.GetEnumerator();
            using var enumerator2 = source2.GetEnumerator();
            while (true)
            {
                var moved1 = enumerator1.MoveNext();
                var moved2 = enumerator2.MoveNext();
                if (moved1 != moved2) return false; // Weird, count were equal. This should not happend
                if (!moved1) return true;
                if (!comparer.Equals(enumerator1.Current, enumerator2.Current)) return false;
            }
        }

        public static bool IsEqualTo<T>(this IReadOnlyList<T> source1, IReadOnlyList<T> source2, IEqualityComparer<T> comparer = null)
        {
            if ((source1 == null) != (source2 == null)) return false;
            if (source1 == null) return true;

            if (source1 is T[] array1 && source2 is T[] array2)
            {
                return array1.IsEqualTo(array2, comparer);
            }

            if (source1.Count != source2.Count) return false;

            var count = source1.Count;
            comparer = comparer ?? EqualityComparer<T>.Default;
            for (int i = 0; i < count; i++)
            {
                if (!comparer.Equals(source1[i], source2[i])) return false;
            }
            return true;
        }

        public static bool IsEqualTo<T>(this T[] source1, T[] source2, IEqualityComparer<T> comparer = null)
        {
            if ((source1 == null) != (source2 == null)) return false;
            if (source1 == null) return true;

            return ((IStructuralEquatable)source1).Equals(other: source2, comparer: (IEqualityComparer)comparer ?? EqualityComparer<T>.Default);
        }



        public static int GetCollectionHashCode<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer = null)
        {
            if (source == null) return 382720733; // Null hash code. I picked a large prime number

            if (source is IReadOnlyCollection<T> collection)
            {
                return collection.GetCollectionHashCode(comparer);
            }

            if (source is IReadOnlyList<T> array)
            {
                return array.GetCollectionHashCode(comparer);
            }

            var code = new HashCode();
            comparer = comparer ?? EqualityComparer<T>.Default;
            using var enumerator = source.GetEnumerator();
            while (true)
            {
                if (!enumerator.MoveNext()) break;
                code.Add(enumerator.Current, comparer);
            }
            return code.ToHashCode();
        }

        public static int GetCollectionHashCode<T>(this IReadOnlyCollection<T> source, IEqualityComparer<T> comparer = null)
        {
            if (source == null) return 382720733; // Null hash code. I picked a large prime number

            if (source is IReadOnlyList<T> array)
            {
                return array.GetCollectionHashCode(comparer);
            }

            var code = new HashCode();
            comparer = comparer ?? EqualityComparer<T>.Default;
            using var enumerator = source.GetEnumerator();
            while (true)
            {
                if (!enumerator.MoveNext()) break;
                code.Add(enumerator.Current, comparer);
            }
            return code.ToHashCode();
        }

        public static int GetCollectionHashCode<T>(this IReadOnlyList<T> source, IEqualityComparer<T> comparer = null)
        {
            if (source == null) return 382720733; // Null hash code. I picked a large prime number

            if (source is T[] array)
            {
                return array.GetCollectionHashCode(comparer);
            }

            var code = new HashCode();
            comparer = comparer ?? EqualityComparer<T>.Default;
            var count = source.Count;
            for (int i = 0; i < count; i++)
            {
                code.Add(source[i], comparer);
            }
            return code.ToHashCode();
        }

        public static int GetCollectionHashCode<T>(this T[] source, IEqualityComparer<T> comparer = null)
        {
            if (source == null) return 382720733; // Null hash code. I picked a large prime number

            return ((IStructuralEquatable)source).GetHashCode(comparer: (IEqualityComparer)comparer ?? EqualityComparer<T>.Default);
        }


        public static IEnumerable<T> CyclicGraphTraverse<T, K>(
            this IEnumerable<T> source,
            bool issueFirst,
            Func<T, IEnumerable<T>> getChildren,
            Func<T, K> getKey,
            IEqualityComparer<K> comparer = null)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (getChildren is null)
            {
                throw new ArgumentNullException(nameof(getChildren));
            }

            if (getKey is null)
            {
                throw new ArgumentNullException(nameof(getKey));
            }

            comparer ??= EqualityComparer<K>.Default;
            var keys = new HashSet<K>(comparer);

            return source.SelectMany(item => Loop(item));

            IEnumerable<T> Loop(T current)
            {
                var key = getKey(current);
                if (!keys.Contains(key))
                {
                    keys.Add(key);
                    if (issueFirst)
                    {
                        yield return current;
                    }

                    var children = getChildren(current);
                    if (children != null)
                    {
                        foreach (var item in children.SelectMany(item => Loop(item)))
                        {
                            yield return item;
                        }
                    }

                    if (!issueFirst)
                    {
                        yield return current;
                    }
                }
            }
        }

        public static IEnumerable<T> CyclicGraphTraverse<T>(
            this IEnumerable<T> source,
            bool issueFirst,
            Func<T, IEnumerable<T>> getChildren,
            IEqualityComparer<T> comparer = null)
        {
            return CyclicGraphTraverse<T, T>(source, issueFirst, getChildren, Funcs.Identity, comparer);
        }

        public static IEnumerable<T> CyclicGraphTraverse<T, K>(
            this T source,
            bool issueFirst,
            Func<T, IEnumerable<T>> getChildren,
            Func<T, K> getKey,
            IEqualityComparer<K> comparer = null) =>
            source.Singleton().CyclicGraphTraverse(issueFirst, getChildren, getKey, comparer);

        public static IEnumerable<T> CyclicGraphTraverse<T>(
            this T source,
            bool issueFirst,
            Func<T, IEnumerable<T>> getChildren,
            IEqualityComparer<T> comparer = null) =>
            source.Singleton().CyclicGraphTraverse(issueFirst, getChildren, comparer);


        public static IEnumerable<T> DepthFirstSearch<T, K>(
            this IEnumerable<T> source,
            Func<T, IEnumerable<T>> getChildren,
            Func<T, K> getKey,
            IEqualityComparer<K> comparer = null) =>
            source.CyclicGraphTraverse(true, getChildren, getKey, comparer);

        public static IEnumerable<T> DepthFirstSearch<T>(
            this IEnumerable<T> source,
            Func<T, IEnumerable<T>> getChildren,
            IEqualityComparer<T> comparer = null) =>
            source.CyclicGraphTraverse(true, getChildren, comparer);

        public static IEnumerable<T> DepthFirstSearch<T, K>(
            this T source,
            Func<T, IEnumerable<T>> getChildren,
            Func<T, K> getKey,
            IEqualityComparer<K> comparer = null) =>
            source.CyclicGraphTraverse(true, getChildren, getKey, comparer);

        public static IEnumerable<T> DepthFirstSearch<T>(
            this T source,
            Func<T, IEnumerable<T>> getChildren,
            IEqualityComparer<T> comparer = null) =>
            source.CyclicGraphTraverse(true, getChildren, comparer);


        public static IEnumerable<T> DepthLastSearch<T, K>(
            this IEnumerable<T> source,
            Func<T, IEnumerable<T>> getChildren,
            Func<T, K> getKey,
            IEqualityComparer<K> comparer = null) =>
            source.CyclicGraphTraverse(false, getChildren, getKey, comparer);

        public static IEnumerable<T> DepthLastSearch<T>(
            this IEnumerable<T> source,
            Func<T, IEnumerable<T>> getChildren,
            IEqualityComparer<T> comparer = null) =>
            source.CyclicGraphTraverse(false, getChildren, comparer);

        public static IEnumerable<T> DepthLastSearch<T, K>(
            this T source,
            Func<T, IEnumerable<T>> getChildren,
            Func<T, K> getKey,
            IEqualityComparer<K> comparer = null) =>
            source.CyclicGraphTraverse(false, getChildren, getKey, comparer);

        public static IEnumerable<T> DepthLastSearch<T>(
            this T source,
            Func<T, IEnumerable<T>> getChildren,
            IEqualityComparer<T> comparer = null) =>
            source.CyclicGraphTraverse(false, getChildren, comparer);


        public static IEnumerable<T> BreadthFirstSearch<T, K>(
            this IEnumerable<T> source,
            Func<T, IEnumerable<T>> getChildren,
            Func<T, K> getKey,
            IEqualityComparer<K> comparer = null)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (getChildren is null)
            {
                throw new ArgumentNullException(nameof(getChildren));
            }

            if (getKey is null)
            {
                throw new ArgumentNullException(nameof(getKey));
            }

            comparer ??= EqualityComparer<K>.Default;
            var keys = new HashSet<K>(comparer);
            var queue = new Queue<T>();

            EnqueueNewItems(source);

            while(queue.TryDequeue(out var current))
            {
                yield return current;

                EnqueueNewItems(getChildren(current));
            }

            void EnqueueNewItems(IEnumerable<T> itemSource)
            {
                if (itemSource != null)
                {
                    foreach (var item in itemSource)
                    {
                        var key = getKey(item);
                        if (!keys.Contains(key))
                        {
                            keys.Add(key);
                            queue.Enqueue(item);
                        }
                    }
                }
            }
        }

        public static IEnumerable<T> BreadthFirstSearch<T>(
            this IEnumerable<T> source,
            Func<T, IEnumerable<T>> getChildren,
            IEqualityComparer<T> comparer = null) =>
            source.BreadthFirstSearch(getChildren, Funcs.Identity, comparer);

        public static IEnumerable<T> BreadthFirstSearch<T, K>(
            this T source,
            Func<T, IEnumerable<T>> getChildren,
            Func<T, K> getKey,
            IEqualityComparer<K> comparer = null) =>
            source.Singleton().BreadthFirstSearch(getChildren, getKey, comparer);

        public static IEnumerable<T> BreadthFirstSearch<T>(
            this T source,
            Func<T, IEnumerable<T>> getChildren,
            IEqualityComparer<T> comparer = null) =>
            source.Singleton().BreadthFirstSearch(getChildren, Funcs.Identity, comparer);


        public static IEnumerable<T> Unfold<T, K>(
            this T source,
            Func<T, T> getNext,
            Func<T, K> getKey,
            IEqualityComparer<K> comparer = null)
            where T : class
        {
            return source.DepthFirstSearch(
                current =>
                {
                    var next = getNext(current);
                    return next != null ? next.Singleton() : null;
                },
                getKey,
                comparer);
        }

        public static IEnumerable<T> Unfold<T>(
            this T source,
            Func<T, T> getNext,
            IEqualityComparer<T> comparer = null)
            where T : class
        {
            return source.Unfold(getNext, Funcs.Identity, comparer);
        }
    }

    public class StructuralEnumerableEqualityComparer<T> : IEqualityComparer<IEnumerable<T>>, IEqualityComparer
    {
        public static IEqualityComparer<IEnumerable<T>> Default = new StructuralEnumerableEqualityComparer<T>();
        private readonly IEqualityComparer<T> itemComparer;

        public StructuralEnumerableEqualityComparer(IEqualityComparer<T> itemComparer = null)
        {
            this.itemComparer = itemComparer;
        }

        public bool Equals(IEnumerable<T> x, IEnumerable<T> y) => x.IsEqualTo(y, itemComparer);

        public int GetHashCode(IEnumerable<T> obj) => obj.GetCollectionHashCode(itemComparer);

        bool IEqualityComparer.Equals(object x, object y) => Equals((IEnumerable<T>)x, (IEnumerable<T>)y);

        int IEqualityComparer.GetHashCode(object obj) => GetHashCode((IEnumerable<T>)obj);
    }

    public class StructuralReadOnlyCollectionEqualityComparer<T> : IEqualityComparer<IReadOnlyCollection<T>>, IEqualityComparer
    {
        public static IEqualityComparer<IReadOnlyCollection<T>> Default = new StructuralEnumerableEqualityComparer<T>();
        private readonly IEqualityComparer<T> itemComparer;

        public StructuralReadOnlyCollectionEqualityComparer(IEqualityComparer<T> itemComparer = null)
        {
            this.itemComparer = itemComparer;
        }

        public bool Equals(IReadOnlyCollection<T> x, IReadOnlyCollection<T> y) => x.IsEqualTo(y, itemComparer);

        public int GetHashCode(IReadOnlyCollection<T> obj) => obj.GetCollectionHashCode(itemComparer);

        bool IEqualityComparer.Equals(object x, object y) => Equals((IReadOnlyCollection<T>)x, (IReadOnlyCollection<T>)y);

        int IEqualityComparer.GetHashCode(object obj) => GetHashCode((IReadOnlyCollection<T>)obj);
    }

    public class StructuralReadOnlyListEqualityComparer<T> : IEqualityComparer<IReadOnlyList<T>>, IEqualityComparer
    {
        public static IEqualityComparer<IReadOnlyList<T>> Default = new StructuralEnumerableEqualityComparer<T>();
        private readonly IEqualityComparer<T> itemComparer;

        public StructuralReadOnlyListEqualityComparer(IEqualityComparer<T> itemComparer = null)
        {
            this.itemComparer = itemComparer;
        }

        public bool Equals(IReadOnlyList<T> x, IReadOnlyList<T> y) => x.IsEqualTo(y, itemComparer);

        public int GetHashCode(IReadOnlyList<T> obj) => obj.GetCollectionHashCode(itemComparer);

        bool IEqualityComparer.Equals(object x, object y) => Equals((IReadOnlyList<T>)x, (IReadOnlyList<T>)y);

        int IEqualityComparer.GetHashCode(object obj) => GetHashCode((IReadOnlyList<T>)obj);
    }

    public class StructuralArrayEqualityComparer<T> : IEqualityComparer<T[]>, IEqualityComparer
    {
        public static IEqualityComparer<T[]> Default = new StructuralArrayEqualityComparer<T>();
        private readonly IEqualityComparer<T> itemComparer;

        public StructuralArrayEqualityComparer(IEqualityComparer<T> itemComparer = null)
        {
            this.itemComparer = itemComparer;
        }

        public bool Equals(T[] x, T[] y) => x.IsEqualTo(y, itemComparer);

        public int GetHashCode(T[] obj) => obj.GetCollectionHashCode(itemComparer);

        bool IEqualityComparer.Equals(object x, object y) => Equals((T[])x, (T[])y);

        int IEqualityComparer.GetHashCode(object obj) => GetHashCode((T[])obj);
    }

}

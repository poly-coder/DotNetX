using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DotNetX
{
    public static class EnumerableExtensions
    {

        #region [ Singleton ]

        public static IEnumerable<T> Singleton<T>(this T value)
        {
            yield return value;
        }

        public static IEnumerable<T> SingletonIf<T>(this T value, Func<T, bool> predicate) 
        {
            if (predicate is null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            if (predicate(value))
                yield return value;
        }

        public static IEnumerable<T> SingletonNonNull<T>(this T? value) 
            where T : class
        {
            if (value != null)
                yield return value!;
        }

        #endregion [ Singleton ]


        #region [ Concatenate ]

        public static string Concatenate(this IEnumerable<string> source, string separator = ", ")
        {
            return String.Join(separator, source);
        }

        #endregion [ Concatenate ]


        #region [ Do ]

        public static IEnumerable<T> Do<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            return source.Select(e =>
            {
                action(e);
                return e;
            });
        }

        #endregion [ Do ]


        #region [ ForEach ]

        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            foreach (var item in source)
            {
                action(item);
            }
        }

        public static async Task ForEachAsync<T>(
            this IEnumerable<T> source, 
            Func<T, CancellationToken, Task> action, 
            CancellationToken cancellationToken = default)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            foreach (var item in source)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await action(item, cancellationToken);
            }
        }

        public static Task ForEachAsync<T>(
            this IEnumerable<T> source,
            Func<T, Task> action,
            CancellationToken cancellationToken = default) =>
            source.ForEachAsync(
                (v, _ct) => action(v),
                cancellationToken);

        public static async Task ParallelForEachAsync<T>(
            this IEnumerable<T> source, 
            Func<T, CancellationToken, Task> action, 
            CancellationToken cancellationToken = default)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            await Task.WhenAll(source.Select(item => action(item, cancellationToken)));
        }

        public static Task ParallelForEachAsync<T>(
            this IEnumerable<T> source,
            Func<T, Task> action,
            CancellationToken cancellationToken = default) =>
            source.ParallelForEachAsync(
                (v, _ct) => action(v),
                cancellationToken);

        #endregion [ ForEach ]


        #region [ StartWith ]

        public static IEnumerable<T> StartWith<T>(this IEnumerable<T> source, IEnumerable<T> toPrepend)
        {
            return toPrepend.Concat(source);
        }

        public static IEnumerable<T> StartWith<T>(this IEnumerable<T> source, params T[] toPrepend)
        {
            return toPrepend.Concat(source);
        }

        #endregion [ StartWith ]


        #region [ OrDefault ]

        public static IEqualityComparer<T> OrDefault<T>(this IEqualityComparer<T>? comparer) =>
            comparer ?? EqualityComparer<T>.Default;

        public static IComparer<T> OrDefault<T>(this IComparer<T>? comparer) =>
            comparer ?? Comparer<T>.Default;

        public static T[] OrDefault<T>(this T[]? array) =>
            array ?? Array.Empty<T>();

        #endregion [ OrDefault ]


        #region [ IndexOf / LastIndexOf ]

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

        public static int IndexOf<T>(this IReadOnlyList<T> source, T item, IEqualityComparer<T>? comparer = null)
        {
            comparer ??= EqualityComparer<T>.Default;
            return source.IndexOf(e => comparer.Equals(e, item));
        }

        public static int LastIndexOf<T>(this IReadOnlyList<T> source, Func<T, bool> predicate)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (predicate is null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            for (int i = source.Count - 1; i >= 0; i--)
            {
                if (predicate(source[i]))
                {
                    return i;
                }
            }
            return -1;
        }

        public static int LastIndexOf<T>(this IReadOnlyList<T> source, T item, IEqualityComparer<T>? comparer = null)
        {
            comparer ??= EqualityComparer<T>.Default;
            return source.LastIndexOf(e => comparer.Equals(e, item));
        }

        #endregion [ IndexOf / LastIndexOf ]


        #region [ IsEqualTo / GetCollectionHashCode ]

        public static bool IsEqualTo<T>(this IEnumerable<T>? source1, IEnumerable<T>? source2, IEqualityComparer<T>? comparer = null)
        {
            if ((source1 == null) != (source2 == null)) return false;
            if (source1 == null) return true;

            if (source1 is IReadOnlyCollection<T> collection1 && source2 is IReadOnlyCollection<T> collection2)
            {
                return collection1.IsEqualTo(collection2, comparer);
            }

            comparer ??= EqualityComparer<T>.Default;
            return AreEqualEnumerables(source1, source2!, comparer);
        }

        public static bool IsEqualTo<T>(this IReadOnlyCollection<T>? source1, IReadOnlyCollection<T>? source2, IEqualityComparer<T>? comparer = null)
        {
            if ((source1 == null) != (source2 == null)) return false;
            if (source1 == null) return true;

            if (source1 is IReadOnlyList<T> list1 && source2 is IReadOnlyList<T> list2)
            {
                return list1.IsEqualTo(list2, comparer);
            }

            if (source1.Count != source2!.Count) return false;

            return AreEqualEnumerables(source1, source2, comparer.OrDefault());
        }

        public static bool IsEqualTo<T>(this IReadOnlyList<T>? source1, IReadOnlyList<T>? source2, IEqualityComparer<T>? comparer = null)
        {
            if ((source1 == null) != (source2 == null)) return false;
            if (source1 == null) return true;

            if (source1 is T[] array1 && source2 is T[] array2)
            {
                return array1.IsEqualTo(array2, comparer);
            }

            if (source1.Count != source2!.Count) return false;

            var count = source1.Count;
            comparer = comparer.OrDefault();
            for (int i = 0; i < count; i++)
            {
                if (!comparer.Equals(source1[i], source2[i])) return false;
            }
            return true;
        }

        public static bool IsEqualTo<T>(this T[]? source1, T[]? source2, IEqualityComparer<T>? comparer = null)
        {
            if ((source1 == null) != (source2 == null)) return false;
            if (source1 == null) return true;

            comparer ??= EqualityComparer<T>.Default;

            return ((IStructuralEquatable)source1).Equals(other: source2, comparer: (IEqualityComparer)comparer);
        }

        private static bool AreEqualEnumerables<T>(IEnumerable<T> source1, IEnumerable<T> source2, IEqualityComparer<T> comparer)
        {
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



        public static int GetCollectionHashCode<T>(this IEnumerable<T>? source, IEqualityComparer<T>? comparer = null)
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

            return ComputeEnumerableHashCode(source, comparer.OrDefault());
        }

        public static int GetCollectionHashCode<T>(this IReadOnlyCollection<T>? source, IEqualityComparer<T>? comparer = null)
        {
            if (source == null) return 382720733; // Null hash code. I picked a large prime number

            if (source is IReadOnlyList<T> array)
            {
                return array.GetCollectionHashCode(comparer);
            }

            return ComputeEnumerableHashCode(source, comparer.OrDefault());
        }

        public static int GetCollectionHashCode<T>(this IReadOnlyList<T>? source, IEqualityComparer<T>? comparer = null)
        {
            if (source == null) return 382720733; // Null hash code. I picked a large prime number

            if (source is T[] array)
            {
                return array.GetCollectionHashCode(comparer);
            }

            var code = new HashCode();
            comparer ??= EqualityComparer<T>.Default;
            var count = source.Count;
            for (int i = 0; i < count; i++)
            {
                code.Add(source[i], comparer);
            }
            return code.ToHashCode();
        }

        public static int GetCollectionHashCode<T>(this T[]? source, IEqualityComparer<T>? comparer = null)
        {
            if (source == null) return 382720733; // Null hash code. I picked a large prime number

            comparer ??= EqualityComparer<T>.Default;

            return ((IStructuralEquatable)source).GetHashCode(comparer: (IEqualityComparer)comparer);
        }

        private static int ComputeEnumerableHashCode<T>(IEnumerable<T> source, IEqualityComparer<T> comparer)
        {
            var code = new HashCode();
            using var enumerator = source.GetEnumerator();
            while (true)
            {
                if (!enumerator.MoveNext()) break;
                code.Add(enumerator.Current, comparer);
            }
            return code.ToHashCode();
        }

        #endregion [ IsEqualTo / GetCollectionHashCode ]

        
        #region [ Graph Traversal ]

        public static IEnumerable<TValue> CyclicGraphTraverse<TValue, TKey>(
            this IEnumerable<TValue> source,
            bool issueFirst,
            Func<TValue, IEnumerable<TValue>?> getChildren,
            Func<TValue, TKey> getKey,
            IEqualityComparer<TKey>? comparer = null)
        {
            comparer ??= EqualityComparer<TKey>.Default;
            var keys = new HashSet<TKey>(comparer);

            return source.SelectMany(item => Loop(item));

            IEnumerable<TValue> Loop(TValue current)
            {
                var key = getKey(current);
                if (!keys!.Contains(key))
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

        public static IEnumerable<TValue> CyclicGraphTraverse<TValue>(
            this IEnumerable<TValue> source,
            bool issueFirst,
            Func<TValue, IEnumerable<TValue>?> getChildren,
            IEqualityComparer<TValue>? comparer = null)
        {
            return CyclicGraphTraverse<TValue, TValue>(source, issueFirst, getChildren, Funcs.Identity, comparer);
        }

        public static IEnumerable<TValue> CyclicGraphTraverse<TValue, TKey>(
            this TValue source,
            bool issueFirst,
            Func<TValue, IEnumerable<TValue>?> getChildren,
            Func<TValue, TKey> getKey,
            IEqualityComparer<TKey>? comparer = null) =>
            source.Singleton().CyclicGraphTraverse(issueFirst, getChildren, getKey, comparer);

        public static IEnumerable<TValue> CyclicGraphTraverse<TValue>(
            this TValue source,
            bool issueFirst,
            Func<TValue, IEnumerable<TValue>?> getChildren,
            IEqualityComparer<TValue>? comparer = null) =>
            source.Singleton().CyclicGraphTraverse(issueFirst, getChildren, comparer);


        public static IEnumerable<TValue> DepthFirstSearch<TValue, TKey>(
            this IEnumerable<TValue> source,
            Func<TValue, IEnumerable<TValue>?> getChildren,
            Func<TValue, TKey> getKey,
            IEqualityComparer<TKey>? comparer = null) =>
            source.CyclicGraphTraverse(true, getChildren, getKey, comparer);

        public static IEnumerable<TValue> DepthFirstSearch<TValue>(
            this IEnumerable<TValue> source,
            Func<TValue, IEnumerable<TValue>?> getChildren,
            IEqualityComparer<TValue>? comparer = null) =>
            source.CyclicGraphTraverse(true, getChildren, comparer);

        public static IEnumerable<TValue> DepthFirstSearch<TValue, TKey>(
            this TValue source,
            Func<TValue, IEnumerable<TValue>?> getChildren,
            Func<TValue, TKey> getKey,
            IEqualityComparer<TKey>? comparer = null) =>
            source.CyclicGraphTraverse(true, getChildren, getKey, comparer);

        public static IEnumerable<TValue> DepthFirstSearch<TValue>(
            this TValue source,
            Func<TValue, IEnumerable<TValue>?> getChildren,
            IEqualityComparer<TValue>? comparer = null) =>
            source.CyclicGraphTraverse(true, getChildren, comparer);


        public static IEnumerable<TValue> DepthLastSearch<TValue, TKey>(
            this IEnumerable<TValue> source,
            Func<TValue, IEnumerable<TValue>?> getChildren,
            Func<TValue, TKey> getKey,
            IEqualityComparer<TKey>? comparer = null) =>
            source.CyclicGraphTraverse(false, getChildren, getKey, comparer);

        public static IEnumerable<TValue> DepthLastSearch<TValue>(
            this IEnumerable<TValue> source,
            Func<TValue, IEnumerable<TValue>?> getChildren,
            IEqualityComparer<TValue>? comparer = null) =>
            source.CyclicGraphTraverse(false, getChildren, comparer);

        public static IEnumerable<TValue> DepthLastSearch<TValue, TKey>(
            this TValue source,
            Func<TValue, IEnumerable<TValue>?> getChildren,
            Func<TValue, TKey> getKey,
            IEqualityComparer<TKey>? comparer = null) =>
            source.CyclicGraphTraverse(false, getChildren, getKey, comparer);

        public static IEnumerable<TValue> DepthLastSearch<TValue>(
            this TValue source,
            Func<TValue, IEnumerable<TValue>?> getChildren,
            IEqualityComparer<TValue>? comparer = null) =>
            source.CyclicGraphTraverse(false, getChildren, comparer);


        public static IEnumerable<TValue> BreadthFirstSearch<TValue, TKey>(
            this IEnumerable<TValue> source,
            Func<TValue, IEnumerable<TValue>?> getChildren,
            Func<TValue, TKey> getKey,
            IEqualityComparer<TKey>? comparer = null)
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

            comparer ??= EqualityComparer<TKey>.Default;
            var keys = new HashSet<TKey>(comparer);
            var queue = new Queue<TValue>();

            EnqueueNewItems(source);

            while(queue.TryDequeue(out var current))
            {
                yield return current;

                var children = getChildren(current);

                if (children != null)
                {
                    EnqueueNewItems(children);
                }
            }

            void EnqueueNewItems(IEnumerable<TValue> itemSource)
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

        public static IEnumerable<TValue> BreadthFirstSearch<TValue>(
            this IEnumerable<TValue> source,
            Func<TValue, IEnumerable<TValue>?> getChildren,
            IEqualityComparer<TValue>? comparer = null) =>
            source.BreadthFirstSearch(getChildren, Funcs.Identity, comparer);

        public static IEnumerable<TValue> BreadthFirstSearch<TValue, TKey>(
            this TValue source,
            Func<TValue, IEnumerable<TValue>?> getChildren,
            Func<TValue, TKey> getKey,
            IEqualityComparer<TKey>? comparer = null) =>
            source.Singleton().BreadthFirstSearch(getChildren, getKey, comparer);

        public static IEnumerable<TValue> BreadthFirstSearch<TValue>(
            this TValue source,
            Func<TValue, IEnumerable<TValue>?> getChildren,
            IEqualityComparer<TValue>? comparer = null) =>
            source.Singleton().BreadthFirstSearch(getChildren, Funcs.Identity, comparer);


        public static IEnumerable<TValue> Unfold<TValue, TKey>(
            this TValue source,
            Func<TValue, TValue> getNext,
            Func<TValue, TKey> getKey,
            Func<TValue, bool> accepted,
            IEqualityComparer<TKey>? comparer = null)
        {
            return source.Singleton().Where(accepted).DepthFirstSearch(
                current =>
                {
                    var next = getNext(current);
                    return accepted(next) ? next.Singleton() : null;
                },
                getKey,
                comparer);
        }

        public static IEnumerable<TValue> Unfold<TValue>(
            this TValue source,
            Func<TValue, TValue> getNext,
            Func<TValue, bool> accepted,
            IEqualityComparer<TValue>? comparer = null)
        {
            return source.Unfold(getNext, Funcs.Identity, accepted, comparer);
        }

        public static IEnumerable<TValue> Unfold<TValue, TKey>(
            this TValue source,
            Func<TValue, TValue?> getNext,
            Func<TValue, TKey> getKey,
            IEqualityComparer<TKey>? comparer = null)
            where TValue : class
        {
            return source.Singleton().Where(Predicate.IsNonNull).DepthFirstSearch(
                current =>
                {
                    var next = getNext(current);
                    return next != null ? next.Singleton() : null;
                },
                getKey,
                comparer);
        }

        public static IEnumerable<TValue> Unfold<TValue>(
            this TValue source,
            Func<TValue, TValue?> getNext,
            IEqualityComparer<TValue>? comparer = null)
            where TValue : class
        {
            return source.Unfold(getNext, Funcs.Identity, comparer);
        }

        #endregion [ Graph Traversal ]


        #region [ ToDictionary ]

        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(
            this IEnumerable<KeyValuePair<TKey, TValue>> source,
            IEqualityComparer<TKey>? comparer = null) 
            where TKey: notnull =>
            new Dictionary<TKey, TValue>(source, comparer.OrDefault());

        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(
            this IEnumerable<Tuple<TKey, TValue>> source,
            IEqualityComparer<TKey>? comparer = null)
            where TKey : notnull =>
            source.ToDictionary(t => t.Item1, t => t.Item2, comparer.OrDefault());

        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(
            this IEnumerable<ValueTuple<TKey, TValue>> source,
            IEqualityComparer<TKey>? comparer = null) 
            where TKey : notnull =>
            source.ToDictionary(t => t.Item1, t => t.Item2, comparer.OrDefault());

        #endregion [ ToDictionary ]


        #region [ AddRange ]

        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            if (collection is null)
            {
                throw new ArgumentNullException(nameof(collection));
            }

            if (items is null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            foreach (var item in items)
            {
                collection.Add(item);
            }
        }

        #endregion


        #region [ AllPairs ]
        
        public static IEnumerable<(T current, T next)> AllPairs<T>(this IEnumerable<T> source)
        {
            var first = true;
            var previous = default(T)!;

            foreach (var current in source)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    yield return (previous, current);
                }

                previous = current;
            }
        }

        #endregion [ AllPairs ]


        #region [ IsSorted / Descending ]

        public static bool IsSorted<T>(this IEnumerable<T> source, IComparer<T>? comparer = null, bool allowEquals = true)
        {
            comparer ??= Comparer<T>.Default;

            bool IsSorted(T a, T b)
            {
                var comparison = comparer.Compare(a, b);

                return comparison < 0 || comparison == 0 && allowEquals;
            }

            return source
                .AllPairs()
                .All(p => IsSorted(p.current, p.next));
        }

        public static bool IsSortedDescending<T>(this IEnumerable<T> source, IComparer<T>? comparer = null, bool allowEquals = true)
        {
            comparer ??= Comparer<T>.Default;

            bool IsSorted(T a, T b)
            {
                var comparison = comparer.Compare(a, b);

                return comparison > 0 || comparison == 0 && allowEquals;
            }

            return source
                .AllPairs()
                .All(p => IsSorted(p.current, p.next));
        }

        #endregion [ IsSorted / Descending ]


        #region [ Cached ]
        
        public static IEnumerable<T> Cached<T>(this IEnumerable<T> source)
        {
            if (source is null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            List<T>? cached = null;
            bool hasMore = true;
            IEnumerator<T>? enumerator = null;

            IEnumerable<T> CachedSource()
            {
                // Go through cached items first

                if (cached != null && cached.Count > 0)
                {
                    foreach (var item in cached)
                    {
                        yield return item;
                    }
                }

                if (!hasMore)
                {
                    yield break;
                }

                // Keep extracting items from the original source

                enumerator ??= source.GetEnumerator();

                while (hasMore = enumerator.MoveNext())
                {
                    var current = enumerator.Current;
                    
                    cached ??= new List<T>();

                    cached.Add(current);

                    yield return current;
                }

                hasMore = false;

                enumerator.Dispose();
            }

            return CachedSource();
        }

        #endregion [ Cached ]


        #region [ Set ]

        public static IEnumerable<T> Subtract<T>(
            this IEnumerable<T> source,
            IEnumerable<T> other,
            IEqualityComparer<T>? comparer = null)
        {
            comparer ??= EqualityComparer<T>.Default;

            var set = new HashSet<T>(other, comparer);

            return source.Where(v => !set.Contains(v));
        }

        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public static IEnumerable<T> OutsideOfInterception<T>(
            this IEnumerable<T> source,
            IEnumerable<T> other,
            IEqualityComparer<T>? comparer = null)
        {
            comparer ??= EqualityComparer<T>.Default;

            source = source.Cached();
            other = other.Cached();

            var intersection = new HashSet<T>(source.Intersect(other, comparer), comparer);

            return source.Concat(other).Where(v => !intersection.Contains(v));
        }

        public static bool HaveSameElements<T>(
            this IEnumerable<T> source,
            IEnumerable<T> other,
            IEqualityComparer<T>? comparer = null)
        {
            return !source.OutsideOfInterception(other, comparer).Any();
        }

        #endregion


        #region [ Windows ]

        public static IEnumerable<IReadOnlyCollection<T>> WindowsSized<T>(
            this IEnumerable<T> source,
            int windowSize,
            bool includeLastPartialWindow = true)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (windowSize <= 0) throw new ArgumentOutOfRangeException(nameof(windowSize));

            var window = new List<T>(windowSize);

            foreach (var item in source)
            {
                window.Add(item);

                if (window.Count >= windowSize)
                {
                    yield return window;

                    window = new List<T>(windowSize);
                }
            }

            if (includeLastPartialWindow && window.Count > 0)
            {
                yield return window;
            }
        }

        #endregion [ Windows ]
    }

    public class StructuralEnumerableEqualityComparer<T> : IEqualityComparer<IEnumerable<T>>, IEqualityComparer
    {
        public static readonly IEqualityComparer<IEnumerable<T>> Default = new StructuralEnumerableEqualityComparer<T>();
        private readonly IEqualityComparer<T> itemComparer;

        public StructuralEnumerableEqualityComparer(IEqualityComparer<T>? itemComparer = null)
        {
            this.itemComparer = itemComparer.OrDefault();
        }

        public bool Equals(IEnumerable<T>? x, IEnumerable<T>? y) => x.IsEqualTo(y, itemComparer);

        public int GetHashCode(IEnumerable<T> obj) => obj.GetCollectionHashCode(itemComparer);

        bool IEqualityComparer.Equals(object? x, object? y) => Equals(x as IEnumerable<T>, y as IEnumerable<T>);

        int IEqualityComparer.GetHashCode(object obj) => GetHashCode((IEnumerable<T>)obj);
    }

    public class StructuralReadOnlyCollectionEqualityComparer<T> : IEqualityComparer<IReadOnlyCollection<T>>, IEqualityComparer
    {
        public static readonly IEqualityComparer<IReadOnlyCollection<T>> Default = new StructuralEnumerableEqualityComparer<T>();
        private readonly IEqualityComparer<T> itemComparer;

        public StructuralReadOnlyCollectionEqualityComparer(IEqualityComparer<T>? itemComparer = null)
        {
            this.itemComparer = itemComparer.OrDefault();
        }

        public bool Equals(IReadOnlyCollection<T>? x, IReadOnlyCollection<T>? y) => x.IsEqualTo(y, itemComparer);

        public int GetHashCode(IReadOnlyCollection<T> obj) => obj.GetCollectionHashCode(itemComparer);

        bool IEqualityComparer.Equals(object? x, object? y) => Equals(x as IReadOnlyCollection<T>, y as IReadOnlyCollection<T>);

        int IEqualityComparer.GetHashCode(object obj) => GetHashCode((IReadOnlyCollection<T>)obj);
    }

    public class StructuralReadOnlyListEqualityComparer<T> : IEqualityComparer<IReadOnlyList<T>>, IEqualityComparer
    {
        public static readonly IEqualityComparer<IReadOnlyList<T>> Default = new StructuralEnumerableEqualityComparer<T>();
        private readonly IEqualityComparer<T> itemComparer;

        public StructuralReadOnlyListEqualityComparer(IEqualityComparer<T>? itemComparer = null)
        {
            this.itemComparer = itemComparer.OrDefault();
        }

        public bool Equals(IReadOnlyList<T>? x, IReadOnlyList<T>? y) => x.IsEqualTo(y, itemComparer);

        public int GetHashCode(IReadOnlyList<T> obj) => obj.GetCollectionHashCode(itemComparer);

        bool IEqualityComparer.Equals(object? x, object? y) => Equals(x as IReadOnlyList<T>, y as IReadOnlyList<T>);

        int IEqualityComparer.GetHashCode(object obj) => GetHashCode((IReadOnlyList<T>)obj);
    }

    public class StructuralArrayEqualityComparer<T> : IEqualityComparer<T[]>, IEqualityComparer
    {
        public static readonly IEqualityComparer<T[]> Default = new StructuralArrayEqualityComparer<T>();
        private readonly IEqualityComparer<T> itemComparer;

        public StructuralArrayEqualityComparer(IEqualityComparer<T>? itemComparer = null)
        {
            this.itemComparer = itemComparer.OrDefault();
        }

        public bool Equals(T[]? x, T[]? y) => x.IsEqualTo(y, itemComparer);

        public int GetHashCode(T[] obj) => obj.GetCollectionHashCode(itemComparer);

        bool IEqualityComparer.Equals(object? x, object? y) => Equals(x as T[], y as T[]);

        int IEqualityComparer.GetHashCode(object obj) => GetHashCode((T[])obj);
    }

}

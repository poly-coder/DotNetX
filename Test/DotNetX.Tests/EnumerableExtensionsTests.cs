using FluentAssertions;
using FsCheck;
using FsCheck.NUnit;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PropertyAttribute = FsCheck.NUnit.PropertyAttribute;

namespace DotNetX.Tests
{
    [TestFixture]
    public class EnumerableExtensionsTests
    {
        #region [ Singleton ]

        [Property]
        public bool SingletonAlwaysReturnAnEnumerabvleWithOneGivenValue(int value)
        {
            value.Singleton().IsEqualTo(new[] { value }).Should().BeTrue();
            return true;
        }

        #endregion [ Singleton ]


        #region [ IsEqualTo / GetCollectionHashCode ]

        [Test]
        public void TwoNullEnumerablesAreEqual()
        {
            IEnumerable<int>? e1 = null;
            IEnumerable<int>? e2 = null;
            e1.IsEqualTo(e2).Should().BeTrue();
        }

        [Property]
        public bool NullEnumerablesIsNotEqualToAnyArray(NonNull<int[]> array)
        {
            IEnumerable<int>? e1 = null;
            e1.IsEqualTo(array.Get).Should().BeFalse();
            array.Get.IsEqualTo(e1).Should().BeFalse();
            return true;
        }

        [Property]
        public bool TwoEnumerablesWithSameContentAreEqual(NonNull<int[]> array)
        {
            array.Get.AsEnumerable().IsEqualTo(array.Get.ToList().AsEnumerable()).Should().BeTrue();
            return true;
        }

        [Property]
        public bool TwoEnumerablesWithDifferentContentAreEqual(NonNull<int[]> array1, NonNull<int[]> array2)
        {
            if (array1.Get.Length != array2.Get.Length)
            {
                CheckArrays(array1.Get, array2.Get);
            }
            else if (array1.Get.Length == 0)
            {
                array1.Get.AsEnumerable().IsEqualTo(array2.Get.AsEnumerable()).Should().BeTrue();
            }
            else
            {
                // Make them differ in a random possition
                var rnd = new System.Random();
                var index = rnd.Next() % (Math.Min(array1.Get.Length, array2.Get.Length));
                if (array1.Get[index] == array2.Get[index])
                {
                    array1.Get[index] = array2.Get[index] + 1;
                }
                CheckArrays(array1.Get, array2.Get);
            }
            return true;

            static void CheckArrays(int[] array1, int[] array2)
            {
                array1.AsEnumerable().IsEqualTo(array2.AsEnumerable()).Should().BeFalse();
                array2.AsEnumerable().IsEqualTo(array1.AsEnumerable()).Should().BeFalse();
            }
        }

        [Property]
        public bool TwoEnumerablesWithSameContentWithComparerAreNotEqual(NonNull<string[]> text)
        {
            var uppercased = text.Get.Select(s => s != null ? s.ToUpperInvariant() : null);
            var lowercased = text.Get.Select(s => s != null ? s.ToLowerInvariant() : null);
            uppercased.IsEqualTo(lowercased, StringComparer.InvariantCultureIgnoreCase).Should().BeTrue();
            return true;
        }


        [Test]
        public void TwoNullCollectionsAreEqual()
        {
            IReadOnlyCollection<int>? e1 = null;
            IReadOnlyCollection<int>? e2 = null;
            e1.IsEqualTo(e2).Should().BeTrue();
        }

        [Property]
        public bool NullCollectionsIsNotEqualToAnyArray(NonNull<int[]> array)
        {
            IReadOnlyCollection<int>? e1 = null;
            e1.IsEqualTo(array.Get).Should().BeFalse();
            array.Get.IsEqualTo(e1).Should().BeFalse();
            return true;
        }

        [Property]
        public bool TwoCollectionsWithSameContentAreEqual(NonNull<int[]> array)
        {
            IReadOnlyCollection<int> collection = new Collection<int>(array.Get);
            collection.IsEqualTo(array.Get).Should().BeTrue();
            return true;
        }

        [Property]
        public bool TwoCollectionsWithDifferentContentAreNotEqual(NonNull<int[]> array1, NonNull<int[]> array2)
        {
            if (array1.Get.Length != array2.Get.Length)
            {
                CheckArrays(array1.Get, array2.Get);
            }
            else if (array1.Get.Length == 0)
            {
                array1.Get.AsEnumerable().IsEqualTo(array2.Get.AsEnumerable()).Should().BeTrue();
            }
            else
            {
                // Make them differ in a random position
                var rnd = new System.Random();
                var index = rnd.Next() % (Math.Min(array1.Get.Length, array2.Get.Length));
                if (array1.Get[index] == array2.Get[index])
                {
                    array1.Get[index] = array2.Get[index] + 1;
                }
                CheckArrays(array1.Get, array2.Get);
            }
            return true;

            static void CheckArrays(int[] array1, int[] array2)
            {
                IReadOnlyCollection<int> collection1 = new Collection<int>(array1);
                IReadOnlyCollection<int> collection2 = new Collection<int>(array2);
                collection1.IsEqualTo(collection2).Should().BeFalse();
                collection2.IsEqualTo(collection1).Should().BeFalse();
            }
        }

        [Property]
        public bool TwoCollectionsWithSameContentWithComparerAreEqual(NonNull<string[]> text)
        {
            IReadOnlyCollection<string> uppercased = new Collection<string>(text.Get.Select(s => s != null ? s.ToUpperInvariant() : null).ToArray());
            var lowercased = text.Get.Select(s => s != null ? s.ToLowerInvariant() : null).ToArray();
            uppercased.IsEqualTo(lowercased, StringComparer.InvariantCultureIgnoreCase).Should().BeTrue();
            return true;
        }

        #endregion [ IsEqualTo / GetCollectionHashCode ]


        #region [ Graph Traversal ]

        [Test]
        public void DepthFirstSearchShouldDealWithRepeatedValuesAndLoops()
        {
            var graph = new Dictionary<int, int[]>
            {
                [1] = new[] { 2, 3, 4 },
                [2] = new[] { 2, 4, 5 },
                [3] = Array.Empty<int>(),
                [4] = new[] { 1, 5 },
            };

            var traversal = graph.Keys.DepthFirstSearch(
                getChildren: i =>
                {
                    if (graph.TryGetValue(i, out var children))
                    {
                        return children;
                    }
                    return null;
                }).ToArray();

            traversal.Should().Equal(1, 2, 4, 5, 3);
        }

        [Test]
        public void DepthFirstSearchFromASingleValue()
        {
            var graph = new Dictionary<int, int[]>
            {
                [1] = new[] { 2, 3, 4 },
                [2] = new[] { 2, 4, 5 },
                [3] = Array.Empty<int>(),
                [4] = new[] { 1, 5 },
            };

            var traversal = 1.DepthFirstSearch(
                getChildren: i =>
                {
                    if (graph.TryGetValue(i, out var children))
                    {
                        return children;
                    }
                    return null;
                }).ToArray();

            traversal.Should().Equal(1, 2, 4, 5, 3);
        }

        [Test]
        public void DepthLastSearchShouldDealWithRepeatedValuesAndLoops()
        {
            var graph = new Dictionary<int, int[]>
            {
                [1] = new[] { 2, 3, 4 },
                [2] = new[] { 2, 4, 5 },
                [3] = Array.Empty<int>(),
                [4] = new[] { 1, 5 },
            };

            var traversal = graph.Keys.DepthLastSearch(
                getChildren: i =>
                {
                    if (graph.TryGetValue(i, out var children))
                    {
                        return children;
                    }
                    return null;
                }).ToArray();

            traversal.Should().Equal(5, 4, 2, 3, 1);
        }

        [Test]
        public void DepthLastSearchFromASingleValue()
        {
            var graph = new Dictionary<int, int[]>
            {
                [1] = new[] { 2, 3, 4 },
                [2] = new[] { 2, 4, 5 },
                [3] = Array.Empty<int>(),
                [4] = new[] { 1, 5 },
            };

            var traversal = 1.DepthLastSearch(
                getChildren: i =>
                {
                    if (graph.TryGetValue(i, out var children))
                    {
                        return children;
                    }
                    return null;
                }).ToArray();

            traversal.Should().Equal(5, 4, 2, 3, 1);
        }

        [Test]
        public void BreadthFirstSearchShouldDealWithRepeatedValuesAndLoops()
        {
            var graph = new Dictionary<int, int[]>
            {
                [1] = new[] { 2, 3, 4 },
                [2] = new[] { 2, 4, 5 },
                [3] = Array.Empty<int>(),
                [4] = new[] { 1, 5 },
            };

            var traversal = graph.Keys.BreadthFirstSearch(
                getChildren: i =>
                {
                    if (graph.TryGetValue(i, out var children))
                    {
                        return children;
                    }
                    return null;
                }).ToArray();

            traversal.Should().Equal(1, 2, 3, 4, 5);
        }

        [Test]
        public void BreadthFirstSearchFromASingleValue()
        {
            var graph = new Dictionary<int, int[]>
            {
                [1] = new[] { 2, 3, 4 },
                [2] = new[] { 2, 4, 5 },
                [3] = Array.Empty<int>(),
                [4] = new[] { 1, 5 },
            };

            var traversal = 1.BreadthFirstSearch(
                getChildren: i =>
                {
                    if (graph.TryGetValue(i, out var children))
                    {
                        return children;
                    }
                    return null;
                }).ToArray();

            traversal.Should().Equal(1, 2, 3, 4, 5);
        }

        [Test]
        public void UnfoldWithLoop()
        {
            var graph = new Dictionary<int, int>
            {
                [1] = 2,
                [2] = 3,
                [3] = 4,
                [4] = 5,
                [5] = 2,
            };

            var traversal = 1.Unfold(
                getNext: i =>
                {
                    if (graph.TryGetValue(i, out var next))
                    {
                        return next;
                    }
                    return 0;
                },
                getKey: i => i,
                accepted: i => i != 0
                ).ToArray();

            traversal.Should().Equal(1, 2, 3, 4, 5);
        }

        [Test]
        public void UnfoldNullableWithLoop()
        {
            var graph = new Dictionary<string, string>
            {
                ["1"] = "2",
                ["2"] = "3",
                ["3"] = "4",
                ["4"] = "5",
                ["5"] = "2",
            };

            var traversal = "1".Unfold(
                getNext: i =>
                {
                    if (graph.TryGetValue(i, out var next))
                    {
                        return next;
                    }
                    return null;
                }).ToArray();

            traversal.Should().Equal("1", "2", "3", "4", "5");
        }

        #endregion [ Graph Traversal ]

    }
}

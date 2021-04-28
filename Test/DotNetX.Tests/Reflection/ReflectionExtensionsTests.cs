using FluentAssertions;
using FsCheck;
using FsCheck.NUnit;
using DotNetX.Reflection;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using PropertyAttribute = FsCheck.NUnit.PropertyAttribute;
using System.Collections;
using System.Threading.Tasks;
using System.ComponentModel.Design;

namespace DotNetX.Tests
{

    [TestFixture]
    public class ReflectionExtensionsTests
    {
        #region [ FormatName / FormatSignature ]

        [Test]
        public void FormatNameOfRegex()
        {
            typeof(Regex).FormatName().Should().Be("Regex");
        }

        [Test]
        public void FullFormatNameOfRegex()
        {
            typeof(Regex).FormatName(true).Should().Be("System.Text.RegularExpressions.Regex");
        }

        [Test]
        public void FullFormatNameOfString()
        {
            typeof(string).FormatName(true).Should().Be("System.String");
        }

        [Test]
        public void FormatNameOfChar()
        {
            typeof(char).FormatName().Should().Be("char");
        }

        [Test]
        public void FormatNameOfBool()
        {
            typeof(bool).FormatName().Should().Be("bool");
        }

        [Test]
        public void FormatNameOfByte()
        {
            typeof(byte).FormatName().Should().Be("byte");
        }

        [Test]
        public void FormatNameOfUShort()
        {
            typeof(ushort).FormatName().Should().Be("ushort");
        }

        [Test]
        public void FormatNameOfUInt()
        {
            typeof(uint).FormatName().Should().Be("uint");
        }

        [Test]
        public void FormatNameOfULong()
        {
            typeof(ulong).FormatName().Should().Be("ulong");
        }

        [Test]
        public void FormatNameOfSByte()
        {
            typeof(sbyte).FormatName().Should().Be("sbyte");
        }

        [Test]
        public void FormatNameOfShort()
        {
            typeof(short).FormatName().Should().Be("short");
        }

        [Test]
        public void FormatNameOfInt()
        {
            typeof(int).FormatName().Should().Be("int");
        }

        [Test]
        public void FormatNameOfLong()
        {
            typeof(long).FormatName().Should().Be("long");
        }

        [Test]
        public void FormatNameOfFloat()
        {
            typeof(float).FormatName().Should().Be("float");
        }

        [Test]
        public void FormatNameOfDouble()
        {
            typeof(double).FormatName().Should().Be("double");
        }

        [Test]
        public void FormatNameOfDecimal()
        {
            typeof(decimal).FormatName().Should().Be("decimal");
        }

        [Test]
        public void FormatNameOfObject()
        {
            typeof(object).FormatName().Should().Be("object");
        }

        [Test]
        public void FormatNameOfVoid()
        {
            typeof(void).FormatName().Should().Be("void");
        }

        [Test]
        public void FormatNameOfListOfString()
        {
            typeof(List<string>).FormatName().Should().Be("List<string>");
        }

        [Test]
        public void FullFormatNameOfListOfString()
        {
            typeof(List<string>).FormatName(true).Should().Be("System.Collections.Generic.List<System.String>");
        }

        [Test]
        public void FormatNameOfListOfT()
        {
            typeof(List<>).FormatName().Should().Be("List<T>");
        }

        [Test]
        public void FullFormatNameOfListOfT()
        {
            typeof(List<>).FormatName(true).Should().Be("System.Collections.Generic.List<T>");
        }

        [Test]
        public void FormatNameOfDictionaryOfStringAndInt()
        {
            typeof(IDictionary<string, int>).FormatName().Should().Be("IDictionary<string, int>");
        }

        [Test]
        public void FullFormatNameOfDictionaryOfStringAndInt()
        {
            typeof(IDictionary<string, int>).FormatName(true).Should().Be("System.Collections.Generic.IDictionary<System.String, System.Int32>");
        }

        [Test]
        public void FormatNameOfDictionaryOfKV()
        {
            typeof(IDictionary<,>).FormatName().Should().Be("IDictionary<TKey, TValue>");
        }

        [Test]
        public void FormatNameOfDateTime()
        {
            typeof(DateTime).FormatName().Should().Be("DateTime");
        }

        [Test]
        public void FormatNameOfArrayOfString()
        {
            typeof(string[]).FormatName().Should().Be("string[]");
        }

        [Test]
        public void FormatNameOfArray2DOfString()
        {
            typeof(string[,]).FormatName().Should().Be("string[,]");
        }

        [Test]
        public void FormatNameOfArray4DOfString()
        {
            typeof(string[,,,]).FormatName().Should().Be("string[,,,]");
        }

        //[Test()]
        //public void FormatNameOfArrayOfArrayOfString()
        //{
        //    typeof(string[,][,,]).FormatName().Should().Be("string[,][,,]");
        //}

        [Test]
        public void FormatNameOfThreadStart()
        {
            typeof(ThreadStart).FormatName().Should().Be("void ThreadStart()");
        }

        [Test]
        public void FullFormatNameOfThreadStart()
        {
            typeof(ThreadStart).FormatName(true).Should().Be("System.Void ThreadStart()");
        }

        #endregion [ FormatName / FormatSignature ]


        #region [ GetClassHierarchy / GetTypeHierarchy ]

        [Test]
        public static void GetClassHierarchyOfClass()
        {
            typeof(DesignerVerbCollection).GetClassHierarchy().Should().Equal(
                typeof(DesignerVerbCollection),
                typeof(CollectionBase),
                typeof(object));
        }

        [Test]
        public static void GetClassHierarchyOfGenericClass()
        {
            typeof(List<string>).GetClassHierarchy().Should().Equal(
                typeof(List<string>),
                typeof(object));
        }

        [Test]
        public static void GetClassHierarchyOfGenericTypeDefinition()
        {
            typeof(List<>).GetClassHierarchy().Should().Equal(
                typeof(List<>),
                typeof(object));
        }

        [Test]
        public static void GetTypeHierarchyOfClass()
        {
            typeof(DesignerVerbCollection).GetTypeHierarchy().Should().Equal(
                typeof(DesignerVerbCollection),
                typeof(CollectionBase),
                typeof(object),
                typeof(IList),
                typeof(ICollection),
                typeof(IEnumerable)
            );
        }

        [Test]
        public static void GetTypeHierarchyOfGenericClass()
        {
            typeof(List<string>).GetTypeHierarchy().Should().BeEquivalentTo(
                typeof(List<string>),
                typeof(object),
                typeof(IList<string>),
                typeof(ICollection<string>),
                typeof(IEnumerable<string>),
                typeof(IReadOnlyList<string>),
                typeof(IReadOnlyCollection<string>),
                typeof(IList),
                typeof(ICollection),
                typeof(IEnumerable)
            );
        }

        [Test]
        public static void BasicReflectionSupposition()
        {
            typeof(List<string>).GetGenericTypeDefinition().Should().Be(typeof(List<>));
            typeof(IList<string>).GetGenericTypeDefinition().Should().Be(typeof(IList<>));
            typeof(List<string>).GetInterfaces().Select(t => t.GetGenericTypeDefinition()).Should().Contain(typeof(IList<>));
        }

        //[Test]
        //public static void GetTypeHierarchyOfGenericTypeDefinition()
        //{
        //    var actual = typeof(List<>).GetTypeHierarchy().ToArray();
        //    var expected = new Type[] {
        //                typeof(List<>),
        //                typeof(object),
        //                typeof(IList<string>).GetGenericTypeDefinition(),
        //                typeof(ICollection<>),
        //                typeof(IEnumerable<>),
        //                typeof(IEnumerable),
        //                typeof(IList),
        //                typeof(ICollection),
        //                typeof(IReadOnlyList<>),
        //                typeof(IReadOnlyCollection<>)
        //            };

        //    actual.Should().Equal(expected);

        //    //typeof(List<>).GetTypeHierarchy().Should().BeEquivalentTo(
        //    //    typeof(List<>),
        //    //    typeof(object),
        //    //    typeof(IList<>),
        //    //    typeof(ICollection<>),
        //    //    typeof(IEnumerable<>),
        //    //    typeof(IEnumerable),
        //    //    typeof(IList),
        //    //    typeof(ICollection),
        //    //    typeof(IReadOnlyList<>),
        //    //    typeof(IReadOnlyCollection<>)
        //    //);
        //}

        #endregion [ GetClassHierarchy / GetTypeHierarchy ]


        #region [ ConformsTo ]

        [Test]
        public static void ConformsToWithNonGenericInterfaces()
        {
            typeof(IList).ConformsTo(typeof(IEnumerable)).Should().BeTrue();
            typeof(IEnumerable).ConformsTo(typeof(IList)).Should().BeFalse();
        }

        [Test]
        public static void ConformsToWithNonGenericClasses()
        {
            typeof(string).ConformsTo(typeof(object)).Should().BeTrue();
            typeof(object).ConformsTo(typeof(string)).Should().BeFalse();
        }

        [Test]
        public static void ConformsToWithOneGenericArg()
        {
            typeof(IList<string>).ConformsTo(typeof(IEnumerable<object>)).Should().BeTrue();
            typeof(IList<object>).ConformsTo(typeof(IEnumerable<string>)).Should().BeFalse();
            typeof(IEnumerable<object>).ConformsTo(typeof(IList<string>)).Should().BeFalse();

            typeof(Task<string>).ConformsTo(typeof(Task<object>)).Should().BeTrue();
            typeof(Task<object>).ConformsTo(typeof(Task<string>)).Should().BeFalse();
        }

        //[Test]
        //public static void ConformsToWithTwoGenericArgs()
        //{
        //    typeof(Dictionary<IList, ICollection>).ConformsTo(typeof(IDictionary<IEnumerable, IEnumerable>)).Should().BeTrue();
        //    //typeof(IDictionary<IEnumerable, IEnumerable>).ConformsTo(typeof(Dictionary<IList, ICollection>)).Should().BeFalse();
        //}


        //[Test]
        //public static void ConformsToWithNonGenericInterfacesPredicate()
        //{
        //    typeof(IEnumerable).ConformsToPredicate()(typeof(IList)).Should().BeTrue();
        //}

        //[Test]
        //public static void ConformsToWithNonGenericClassesPredicate()
        //{
        //    typeof(object).ConformsToPredicate()(typeof(string)).Should().BeTrue();
        //}

        //[Test]
        //public static void ConformsToWithOneGenericArgPredicate()
        //{
        //    typeof(IEnumerable<object>).ConformsToPredicate()(typeof(IList<string>)).Should().BeTrue();
        //    typeof(Task<string>).ConformsTo(typeof(Task<object>)).Should().BeTrue();
        //}

        //[Test]
        //public static void ConformsToWithTwoGenericArgsPredicate()
        //{
        //    typeof(IDictionary<IEnumerable, IEnumerable>).ConformsToPredicate()(typeof(Dictionary<IList, ICollection>)).Should().BeTrue();
        //}

        #endregion [ ConformsTo ]


        [Test]
        public void TryGetAllGenericParametersOnNullGenericTypeShouldFail()
        {
            // Given
            Type type = typeof(string);
            Type genericTypeDefinition = default(Type)!; 

            // When
            Action action = () => type.TryGetAllGenericParameters(genericTypeDefinition, out var types);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void TryGetAllGenericParametersOnNullTypeShouldFail()
        {
            // Given
            Type type = default(Type)!;
            Type genericTypeDefinition = typeof(string);

            // When
            Action action = () => type.TryGetAllGenericParameters(genericTypeDefinition, out var types);

            // Then
            action.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void TryGetAllGenericParametersOnNonGenericTypeShouldReturnFalse()
        {
            // Given
            Type type = typeof(string);
            Type genericTypeDefinition = typeof(string);

            // When
            var result = type.TryGetAllGenericParameters(genericTypeDefinition, out var types);

            // Then
            result.Should().Be(false);
        }

        [Test]
        public void TryGetAllGenericParametersOnNonMatchingTypeShouldReturnFalse()
        {
            // Given
            Type type = typeof(IEnumerable<string>);
            Type genericTypeDefinition = typeof(IDictionary<,>);

            // When
            var result = type.TryGetAllGenericParameters(genericTypeDefinition, out var types);

            // Then
            result.Should().Be(false);
        }

        [Test]
        public void TryGetAllGenericParametersOnExactMatchingTypeShouldReturnTrue()
        {
            // Given
            Type type = typeof(IEnumerable<string>);
            Type genericTypeDefinition = typeof(IEnumerable<>);

            // When
            var result = type.TryGetAllGenericParameters(genericTypeDefinition, out var types);

            // Then
            result.Should().Be(true);
            types.Should().Equal(typeof(string));
        }

        [Test]
        public void TryGetAllGenericParametersOnHierarchyMatchingTypeShouldReturnFalse()
        {
            // Given
            Type type = typeof(List<string>);
            Type genericTypeDefinition = typeof(IEnumerable<>);

            // When
            var result = type.TryGetAllGenericParameters(genericTypeDefinition, out var types);

            // Then
            result.Should().Be(false);
        }

        [Test]
        public void TryGetGenericParameters1OnNonMatchingTypeShouldReturnFalse()
        {
            // Given
            Type type = typeof(IEnumerable<string>);
            Type genericTypeDefinition = typeof(IDictionary<,>);

            // When
            var result = type.TryGetGenericParameters(genericTypeDefinition, out var first);

            // Then
            result.Should().Be(false);
        }

        [Test]
        public void TryGetGenericParameters1OnExactMatchingTypeShouldReturnTrue()
        {
            // Given
            Type type = typeof(IEnumerable<string>);
            Type genericTypeDefinition = typeof(IEnumerable<>);

            // When
            var result = type.TryGetGenericParameters(genericTypeDefinition, out var first);

            // Then
            result.Should().Be(true);
            first.Should().Be(typeof(string));
        }
        
        [Test]
        public void TryGetGenericParameters2OnNonMatchingTypeShouldReturnFalse()
        {
            // Given
            Type type = typeof(IDictionary<string, int>);
            Type genericTypeDefinition = typeof(IEnumerable<>);

            // When
            var result = type.TryGetGenericParameters(genericTypeDefinition, out var _, out var _);

            // Then
            result.Should().Be(false);
        }

        [Test]
        public void TryGetGenericParameters2OnExactMatchingTypeShouldReturnTrue()
        {
            // Given
            Type type = typeof(IDictionary<string, int>);
            Type genericTypeDefinition = typeof(IDictionary<,>);

            // When
            var result = type.TryGetGenericParameters(genericTypeDefinition, out var first, out var second);

            // Then
            result.Should().Be(true);
            first.Should().Be(typeof(string));
            second.Should().Be(typeof(int));
        }
        
        [Test]
        public void TryGetGenericParameters3OnNonMatchingTypeShouldReturnFalse()
        {
            // Given
            Type type = typeof(ISomeInterfaceWithThreeArgs<string, int, DateTime>);
            Type genericTypeDefinition = typeof(IEnumerable<>);

            // When
            var result = type.TryGetGenericParameters(genericTypeDefinition, out var _, out var _, out var _);

            // Then
            result.Should().Be(false);
        }

        [Test]
        public void TryGetGenericParameters3OnExactMatchingTypeShouldReturnTrue()
        {
            // Given
            Type type = typeof(ISomeInterfaceWithThreeArgs<string, int, DateTime>);
            Type genericTypeDefinition = typeof(ISomeInterfaceWithThreeArgs<,,>);

            // When
            var result = type.TryGetGenericParameters(genericTypeDefinition, out var first, out var second, out var third);

            // Then
            result.Should().Be(true);
            first.Should().Be(typeof(string));
            second.Should().Be(typeof(int));
            third.Should().Be(typeof(DateTime));
        }

        public interface ISomeInterfaceWithThreeArgs<A, B, C>
        {

        }
    }
}

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

namespace DotNetX.Tests
{
    [TestFixture]
    public class ReflectionExtensionsTests
    {
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

        // [Test]
        // public void FormatNameOfThreadStart()
        // {
        //     typeof(ThreadStart).FormatName().Should().Be("void ThreadStart()");
        // }
    }
}

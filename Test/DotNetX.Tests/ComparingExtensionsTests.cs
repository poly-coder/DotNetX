using FluentAssertions;
using FsCheck;
using FsCheck.NUnit;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using PropertyAttribute = FsCheck.NUnit.PropertyAttribute;

namespace DotNetX.Tests
{
    public class ComparingExtensionsTests
    {
        [Test]
        public void IsAtLeastForInt32()
        {
            0.IsAtLeast(5).Should().BeFalse();
            4.IsAtLeast(5).Should().BeFalse();
            5.IsAtLeast(5).Should().BeTrue();
            6.IsAtLeast(5).Should().BeTrue();
            9.IsAtLeast(5).Should().BeTrue();
        } 

        [Test]
        public void IsAtLeastExclusiveForInt32()
        {
            0.IsAtLeastExclusive(5).Should().BeFalse();
            4.IsAtLeastExclusive(5).Should().BeFalse();
            5.IsAtLeastExclusive(5).Should().BeFalse();
            6.IsAtLeastExclusive(5).Should().BeTrue();
            9.IsAtLeastExclusive(5).Should().BeTrue();
        } 

        [Test]
        public void IsAtMostForInt32()
        {
            0.IsAtMost(5).Should().BeTrue();
            4.IsAtMost(5).Should().BeTrue();
            5.IsAtMost(5).Should().BeTrue();
            6.IsAtMost(5).Should().BeFalse();
            9.IsAtMost(5).Should().BeFalse();
        } 

        [Test]
        public void IsAtMostExclusiveForInt32()
        {
            0.IsAtMostExclusive(5).Should().BeTrue();
            4.IsAtMostExclusive(5).Should().BeTrue();
            5.IsAtMostExclusive(5).Should().BeFalse();
            6.IsAtMostExclusive(5).Should().BeFalse();
            9.IsAtMostExclusive(5).Should().BeFalse();
        } 

        [Test]
        public void IsBetweenForInt32()
        {
            (-10).IsBetween(0, 10).Should().BeFalse();
            (-1) .IsBetween(0, 10).Should().BeFalse();
            0    .IsBetween(0, 10).Should().BeTrue();
            1    .IsBetween(0, 10).Should().BeTrue();
            5    .IsBetween(0, 10).Should().BeTrue();
            9    .IsBetween(0, 10).Should().BeTrue();
            10   .IsBetween(0, 10).Should().BeTrue();
            11   .IsBetween(0, 10).Should().BeFalse();
            20   .IsBetween(0, 10).Should().BeFalse();
        } 

        [Test]
        public void IsBetweenExclusiveForInt32()
        {
            (-10).IsBetweenExclusive(0, 10).Should().BeFalse();
            (-1) .IsBetweenExclusive(0, 10).Should().BeFalse();
            0    .IsBetweenExclusive(0, 10).Should().BeFalse();
            1    .IsBetweenExclusive(0, 10).Should().BeTrue();
            5    .IsBetweenExclusive(0, 10).Should().BeTrue();
            9    .IsBetweenExclusive(0, 10).Should().BeTrue();
            10   .IsBetweenExclusive(0, 10).Should().BeFalse();
            11   .IsBetweenExclusive(0, 10).Should().BeFalse();
            20   .IsBetweenExclusive(0, 10).Should().BeFalse();
        }

        [Test]
        public void ClampForInt32()
        {
            (-10).Clamp(0, 10).Should().Be(0);
            (-1) .Clamp(0, 10).Should().Be(0);
            0    .Clamp(0, 10).Should().Be(0);
            1    .Clamp(0, 10).Should().Be(1);
            5    .Clamp(0, 10).Should().Be(5);
            9    .Clamp(0, 10).Should().Be(9);
            10   .Clamp(0, 10).Should().Be(10);
            11   .Clamp(0, 10).Should().Be(10);
            20   .Clamp(0, 10).Should().Be(10);
        } 

        [Test]
        public void AtLeastForInt32()
        {
            0.AtLeast(5).Should().Be(5);
            4.AtLeast(5).Should().Be(5);
            5.AtLeast(5).Should().Be(5);
            6.AtLeast(5).Should().Be(6);
            9.AtLeast(5).Should().Be(9);
        } 

        [Test]
        public void AtMostForInt32()
        {
            0.AtMost(5).Should().Be(0);
            4.AtMost(5).Should().Be(4);
            5.AtMost(5).Should().Be(5);
            6.AtMost(5).Should().Be(5);
            9.AtMost(5).Should().Be(5);
        }


        [Test]
        public void IsAtLeastForInt32WithComparer()
        {
            var comparer = Comparer<int>.Default.Inverse();

            0.IsAtLeast(5, comparer).Should().BeTrue();
            4.IsAtLeast(5, comparer).Should().BeTrue();
            5.IsAtLeast(5, comparer).Should().BeTrue();
            6.IsAtLeast(5, comparer).Should().BeFalse();
            9.IsAtLeast(5, comparer).Should().BeFalse();
        }

        [Test]
        public void IsAtLeastExclusiveForInt32WithComparer()
        {
            var comparer = Comparer<int>.Default.Inverse();

            0.IsAtLeastExclusive(5, comparer).Should().BeTrue();
            4.IsAtLeastExclusive(5, comparer).Should().BeTrue();
            5.IsAtLeastExclusive(5, comparer).Should().BeFalse();
            6.IsAtLeastExclusive(5, comparer).Should().BeFalse();
            9.IsAtLeastExclusive(5, comparer).Should().BeFalse();
        }

        [Test]
        public void IsAtMostForInt32WithComparer()
        {
            var comparer = Comparer<int>.Default.Inverse();

            0.IsAtMost(5, comparer).Should().BeFalse();
            4.IsAtMost(5, comparer).Should().BeFalse();
            5.IsAtMost(5, comparer).Should().BeTrue();
            6.IsAtMost(5, comparer).Should().BeTrue();
            9.IsAtMost(5, comparer).Should().BeTrue();
        }

        [Test]
        public void IsAtMostExclusiveForInt32WithComparer()
        {
            var comparer = Comparer<int>.Default.Inverse();

            0.IsAtMostExclusive(5, comparer).Should().BeFalse();
            4.IsAtMostExclusive(5, comparer).Should().BeFalse();
            5.IsAtMostExclusive(5, comparer).Should().BeFalse();
            6.IsAtMostExclusive(5, comparer).Should().BeTrue();
            9.IsAtMostExclusive(5, comparer).Should().BeTrue();
        }

        [Test]
        public void IsBetweenForInt32WithComparer()
        {
            var comparer = Comparer<int>.Default.Inverse();

            (-10).IsBetween(10, 0, comparer).Should().BeFalse();
            (-1).IsBetween(10, 0, comparer).Should().BeFalse();
            0.IsBetween(10, 0, comparer).Should().BeTrue();
            1.IsBetween(10, 0, comparer).Should().BeTrue();
            5.IsBetween(10, 0, comparer).Should().BeTrue();
            9.IsBetween(10, 0, comparer).Should().BeTrue();
            10.IsBetween(10, 0, comparer).Should().BeTrue();
            11.IsBetween(10, 0, comparer).Should().BeFalse();
            20.IsBetween(10, 0, comparer).Should().BeFalse();
        }

        [Test]
        public void IsBetweenExclusiveForInt32WithComparer()
        {
            var comparer = Comparer<int>.Default.Inverse();

            (-10).IsBetweenExclusive(10, 0, comparer).Should().BeFalse();
            (-1).IsBetweenExclusive(10, 0, comparer).Should().BeFalse();
            0.IsBetweenExclusive(10, 0, comparer).Should().BeFalse();
            1.IsBetweenExclusive(10, 0, comparer).Should().BeTrue();
            5.IsBetweenExclusive(10, 0, comparer).Should().BeTrue();
            9.IsBetweenExclusive(10, 0, comparer).Should().BeTrue();
            10.IsBetweenExclusive(10, 0, comparer).Should().BeFalse();
            11.IsBetweenExclusive(10, 0, comparer).Should().BeFalse();
            20.IsBetweenExclusive(10, 0, comparer).Should().BeFalse();
        }

        [Test]
        public void ClampForInt32WithComparer()
        {
            var comparer = Comparer<int>.Default.Inverse();

            (-10).Clamp(10, 0, comparer).Should().Be(0);
            (-1).Clamp(10, 0, comparer).Should().Be(0);
            0.Clamp(10, 0, comparer).Should().Be(0);
            1.Clamp(10, 0, comparer).Should().Be(1);
            5.Clamp(10, 0, comparer).Should().Be(5);
            9.Clamp(10, 0, comparer).Should().Be(9);
            10.Clamp(10, 0, comparer).Should().Be(10);
            11.Clamp(10, 0, comparer).Should().Be(10);
            20.Clamp(10, 0, comparer).Should().Be(10);
        }

        [Test]
        public void AtLeastForInt32WithComparer()
        {
            var comparer = Comparer<int>.Default.Inverse();

            0.AtLeast(5, comparer).Should().Be(0);
            4.AtLeast(5, comparer).Should().Be(4);
            5.AtLeast(5, comparer).Should().Be(5);
            6.AtLeast(5, comparer).Should().Be(5);
            9.AtLeast(5, comparer).Should().Be(5);
        }

        [Test]
        public void AtMostForInt32WithComparer()
        {
            var comparer = Comparer<int>.Default.Inverse();

            0.AtMost(5, comparer).Should().Be(5);
            4.AtMost(5, comparer).Should().Be(5);
            5.AtMost(5, comparer).Should().Be(5);
            6.AtMost(5, comparer).Should().Be(6);
            9.AtMost(5, comparer).Should().Be(9);
        }


        [Test]
        public void IsBetweenForInt64()
        {
            (-10L).IsBetween(0L, 10L).Should().BeFalse();
            (-1L) .IsBetween(0L, 10L).Should().BeFalse();
            0L    .IsBetween(0L, 10L).Should().BeTrue();
            1L    .IsBetween(0L, 10L).Should().BeTrue();
            5L    .IsBetween(0L, 10L).Should().BeTrue();
            9L    .IsBetween(0L, 10L).Should().BeTrue();
            10L   .IsBetween(0L, 10L).Should().BeTrue();
            11L   .IsBetween(0L, 10L).Should().BeFalse();
            20L   .IsBetween(0L, 10L).Should().BeFalse();
        } 

        [Test]
        public void IsBetweenExclusiveForInt64()
        {
            (-10L).IsBetweenExclusive(0L, 10L).Should().BeFalse();
            (-1L) .IsBetweenExclusive(0L, 10L).Should().BeFalse();
            0L    .IsBetweenExclusive(0L, 10L).Should().BeFalse();
            1L    .IsBetweenExclusive(0L, 10L).Should().BeTrue();
            5L    .IsBetweenExclusive(0L, 10L).Should().BeTrue();
            9L    .IsBetweenExclusive(0L, 10L).Should().BeTrue();
            10L   .IsBetweenExclusive(0L, 10L).Should().BeFalse();
            11L   .IsBetweenExclusive(0L, 10L).Should().BeFalse();
            20L   .IsBetweenExclusive(0L, 10L).Should().BeFalse();
        } 


        [Test]
        public void ClampForInt64()
        {
            (-10L).Clamp(0L, 10L).Should().Be(0L);
            (-1L) .Clamp(0L, 10L).Should().Be(0L);
            0L    .Clamp(0L, 10L).Should().Be(0L);
            1L    .Clamp(0L, 10L).Should().Be(1L);
            5L    .Clamp(0L, 10L).Should().Be(5L);
            9L    .Clamp(0L, 10L).Should().Be(9L);
            10L   .Clamp(0L, 10L).Should().Be(10L);
            11L   .Clamp(0L, 10L).Should().Be(10L);
            20L   .Clamp(0L, 10L).Should().Be(10L);
        } 

        [Test]
        public void IsBetweenForDouble()
        {
            (-10D).IsBetween(0D, 10D).Should().BeFalse();
            (-1D) .IsBetween(0D, 10D).Should().BeFalse();
            (-0.01D).IsBetween(0D, 10D).Should().BeFalse();
            0D    .IsBetween(0D, 10D).Should().BeTrue();
            0.001D.IsBetween(0D, 10D).Should().BeTrue();
            1D    .IsBetween(0D, 10D).Should().BeTrue();
            5D    .IsBetween(0D, 10D).Should().BeTrue();
            9D    .IsBetween(0D, 10D).Should().BeTrue();
            9.99D .IsBetween(0D, 10D).Should().BeTrue();
            10D   .IsBetween(0D, 10D).Should().BeTrue();
            10.01D.IsBetween(0D, 10D).Should().BeFalse();
            11D   .IsBetween(0D, 10D).Should().BeFalse();
            20D   .IsBetween(0D, 10D).Should().BeFalse();
        } 

        [Test]
        public void IsBetweenExclusiveForDouble()
        {
            (-10D).IsBetweenExclusive(0D, 10D).Should().BeFalse();
            (-1D) .IsBetweenExclusive(0D, 10D).Should().BeFalse();
            (-0.01D).IsBetweenExclusive(0D, 10D).Should().BeFalse();
            0D    .IsBetweenExclusive(0D, 10D).Should().BeFalse();
            0.01D .IsBetweenExclusive(0D, 10D).Should().BeTrue();
            1D    .IsBetweenExclusive(0D, 10D).Should().BeTrue();
            5D    .IsBetweenExclusive(0D, 10D).Should().BeTrue();
            9D    .IsBetweenExclusive(0D, 10D).Should().BeTrue();
            9.99D .IsBetweenExclusive(0D, 10D).Should().BeTrue();
            10D   .IsBetweenExclusive(0D, 10D).Should().BeFalse();
            10.01D.IsBetweenExclusive(0D, 10D).Should().BeFalse();
            11D   .IsBetweenExclusive(0D, 10D).Should().BeFalse();
            20D   .IsBetweenExclusive(0D, 10D).Should().BeFalse();
        } 

        [Test]
        public void ClampForDouble()
        {
            (-10D).Clamp(0D, 10D).Should().Be(0D);
            (-1D) .Clamp(0D, 10D).Should().Be(0D);
            (-0.01D).Clamp(0D, 10D).Should().Be(0D);
            0D    .Clamp(0D, 10D).Should().Be(0D);
            0.01D.Clamp(0D, 10D).Should().Be(0.01D);
            1D    .Clamp(0D, 10D).Should().Be(1D);
            5D    .Clamp(0D, 10D).Should().Be(5D);
            9D    .Clamp(0D, 10D).Should().Be(9D);
            9.99D .Clamp(0D, 10D).Should().Be(9.99D);
            10D   .Clamp(0D, 10D).Should().Be(10D);
            10.01D.Clamp(0D, 10D).Should().Be(10D);
            11D   .Clamp(0D, 10D).Should().Be(10D);
            20D   .Clamp(0D, 10D).Should().Be(10D);
        } 
    }
}

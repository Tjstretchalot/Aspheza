using NUnit.Framework;
using BaseBuilder.Engine.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseBuilder.Engine.Logic.Tests
{
    [TestFixture()]
    public class LogicUtilsTests
    {
        [Test()]
        public void BinaryInsertTest()
        {
            Func<int, int, int> comparer = (i1, i2) => i1 - i2;

            var arr = new List<int> { 2, 4, 6, 8, 10 };
            var testArr = new List<int> { 2, 4, 6, 8, 10 };

            LogicUtils.BinaryInsert(arr, 3, comparer);
            testArr.Insert(1, 3);

            Assert.That(testArr.SequenceEqual(arr), $"{testArr} expected but got {arr}");

            LogicUtils.BinaryInsert(arr, 1, comparer);
            testArr.Insert(0, 1);
            
            Assert.That(testArr.SequenceEqual(arr), $"{testArr} expected but got {arr}");

            LogicUtils.BinaryInsert(arr, 11, comparer);
            testArr.Add(11);

            Assert.That(testArr.SequenceEqual(arr), $"{testArr} expected but got {arr}");
        }
    }
}
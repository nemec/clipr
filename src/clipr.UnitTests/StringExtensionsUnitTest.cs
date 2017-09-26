using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using clipr.Utils;

namespace clipr.UnitTests
{
    [TestClass]
    public class StringExtensionsUnitTest
    {
        [TestMethod]
        public void Reflow_WithTextLongerThanMax_SplitsIntoTwoLines()
        {
            var expected = new List<int> { 5, 2 };
            var actual = "1234567".Reflow(5).Select(t => t.Length).ToList();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Reflow_WithTextShorterThanMax_DoesNotSplit()
        {
            var expected = new List<int> { 3 };
            var actual = "123".Reflow(5).Select(t => t.Length).ToList();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Reflow_WithTextSpanningMultipleLines_SplitsIntoManyLines()
        {
            var expected = new List<int> { 5, 5, 5 };
            var actual = "123451234512345".Reflow(5).Select(t => t.Length).ToList();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Reflow_WithAllSpaces_SplitsIntoManyLines()
        {
            var expected = new List<int> { 5, 5, 5 };
            var actual = "               ".Reflow(5).Select(t => t.Length).ToList();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ReflowWords_WithAllSpaces_ReturnsNothing()
        {
            var expected = new List<int>();
            var actual = "               ".ReflowWords(5).Select(t => t.Length).ToList();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ReflowWords_WithSeparatorOnWordBoundary_ReturnsSplitWords()
        {
            var expected = new List<int>{ 4, 5 };
            var actual = "some other".ReflowWords(5).Select(t => t.Length).ToList();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ReflowWords_WithMultipleSeparatorsOnWordBoundary_ReturnsSplitWords()
        {
            var expected = new List<int> { 3, 4 };
            var actual = "som   ther".ReflowWords(5).Select(t => t.Length).ToList();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ReflowWords_WithEmptyLineInMiddle_SkipsEmptyLine()
        {
            var expected = new List<int> { 4, 5 };
            var actual = "some      other".ReflowWords(5).Select(t => t.Length).ToList();
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ReflowWords_WithMultipleWordsPerLine_ReflowsLines()
        {
            var expected = new List<int> { 9, 10, 4 };
            var actual = "now there is another line".ReflowWords(10).Select(t => t.Length).ToList();
            CollectionAssert.AreEqual(expected, actual);
        }
    }
}

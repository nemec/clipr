using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using clipr.Utils;

namespace clipr.UnitTests
{
    [TestClass]
    public class TokenizerUnitTest
    {
        [TestMethod]
        public void Tokenize_WithMultipleStandardArguments_TokenizesArguments()
        {
            var expected = new[] { "MyApp", "alpha", "beta" };

            var actual = Tokenizer.Tokenize("MyApp alpha beta");

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Tokenize_WithArgumentsContainingDoubleQuotedSpaces_TokenizesArguments()
        {
            var expected = new[] { "MyApp", "alpha with spaces", "beta with spaces" };

            var actual = Tokenizer.Tokenize(@"MyApp ""alpha with spaces"" ""beta with spaces""");

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Tokenize_WithArgumentsContainingSingleQuotedSpaces_TokenizesArgumentsAsUnquoted()
        {
            var expected = new[] { "MyApp", "'alpha", "with", "spaces'", "beta" };

            var actual = Tokenizer.Tokenize("MyApp 'alpha with spaces' beta");

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Tokenize_WithArgumentsContainingEscapedDoubleQuote_TokenizesArguments()
        {
            var expected = new[] { "MyApp", @"\\\alpha", @"\\beta" };

            var actual = Tokenizer.Tokenize(@"MyApp \\\alpha \\\\""beta");

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Tokenize_WithArgumentsContainingEscapedSlashes_TokenizesArguments()
        {
            var expected = new[] { "MyApp", @"\\""alpha", @"""beta" };

            var actual = Tokenizer.Tokenize(@"MyApp \\\\\""alpha \""beta");

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Tokenize_WithQuotedArgumentsContainingLeadingWhitespace_TokenizesArguments()
        {
            var expected = new[] { "\t\thello" };

            var actual = Tokenizer.Tokenize("\"\t\thello\"");

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Tokenize_WithQuotedArgumentsContainingTrailingWhitespace_TokenizesArguments()
        {
            var expected = new[] { "hello\t\t" };

            var actual = Tokenizer.Tokenize("\"hello\t\t\"");

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Tokenize_WithLeadingSlashArgument_TokenizesArguments()
        {
            var expected = new[] { @"\hello" };

            var actual = Tokenizer.Tokenize(@"\hello");

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Tokenize_WithFilePathArgument_TokenizesArguments()
        {
            var expected = new[] {
                @"/src:C:\tmp\Some Folder\Sub Folder",
                @"/users:abcdefg@hijkl.com",
                @"tasks:SomeTask,Some Other Task",
                @"-someParam",
                @"foo"
            };

            var actual = Tokenizer.Tokenize(@"/src:""C:\tmp\Some Folder\Sub Folder"" /users:""abcdefg@hijkl.com"" tasks:""SomeTask,Some Other Task"" -someParam foo");

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Tokenize_WithWeirdQuotes_TokenizesArguments()
        {
            var expected = new[] {
                @"ab cd",
                "e",
                @"f"
            };

            var actual = Tokenizer.Tokenize(@"a""b c""d e f");

            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Tokenize_WithSustainingQuotes_TokenizesArguments()
        {
            var expected = new[] { "ab cdef gh" };

            var actual = Tokenizer.Tokenize(@"a""b c""de""f gh""");


            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void Tokenize_WithTrailingBackslash_TokenizesArguments()
        {
            var expected = new[] { "head", "-p", @"c:""" };

            var actual = Tokenizer.Tokenize(@"head -p ""c:\""");


            CollectionAssert.AreEqual(expected, actual);
        }
    }
}

using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace clipr.UnitTests
{
    [TestClass]
    public class ParseActionUnitTest
    {
#region Count Action

        internal class NamedArgumentCount
        {
            [NamedArgument('v', "verbose", Action = ParseAction.Count)]
            public int Verbosity { get; set; }
        }

        [TestMethod]
        public void Argument_WithParseActionCount_AccumulatesCount()
        {
            var result = CliParser.Parse<NamedArgumentCount>("-v -v -v".Split());
            result.Handle(
                opt => Assert.AreEqual(3, opt.Verbosity),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

#endregion

#region StoreTrue Action

        internal class StoreTrueAction
        {
            [NamedArgument('b', Action = ParseAction.StoreTrue)]
            public bool IsSet { get; set; }
        }

        [TestMethod]
        public void Argument_WithStoreTrueAction_StoresBoolAsTrue()
        {
            var result = CliParser.Parse<StoreTrueAction>("-b".Split());
            result.Handle(
                opt => Assert.IsTrue(opt.IsSet),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        internal class StoreTrueActionNullable
        {
            [NamedArgument('b', Action = ParseAction.StoreTrue)]
            public bool? IsSet { get; set; }
        }

        [TestMethod]
        public void Argument_WithStoreTrueActionAndNullable_StoresBoolAsTrue()
        {
            var result = CliParser.Parse<StoreTrueActionNullable>("-b".Split());
            result.Handle(
                opt => Assert.IsTrue(opt.IsSet.GetValueOrDefault()),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

#endregion

#region StoreFalse Action

        internal class StoreFalseAction
        {
            public StoreFalseAction()
            {
                IsSet = true;
            }

            [NamedArgument('b', Action = ParseAction.StoreFalse)]
            public bool IsSet { get; set; }
        }

        [TestMethod]
        public void Argument_WithStoreFalseAction_StoresBoolAsFalse()
        {
            var result = CliParser.Parse<StoreFalseAction>("-b".Split());
            result.Handle(
                opt => Assert.IsFalse(opt.IsSet),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        internal class StoreFalseActionNullable
        {
            [NamedArgument('b', Action = ParseAction.StoreFalse)]
            public bool? IsSet { get; set; }
        }

        [TestMethod]
        public void Argument_WithStoreFalseActionAndNullable_StoresBoolAsFalse()
        {
            var result = CliParser.Parse<StoreFalseActionNullable>("-b".Split());
            result.Handle(
                opt => Assert.IsFalse(opt.IsSet.GetValueOrDefault()),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        [TestMethod]
        public void Argument_WithStoreFalseActionAndNullableAndFlagNotProvided_StoresBoolAsNull()
        {
            var result = CliParser.Parse<StoreFalseActionNullable>(new string[] { });
            result.Handle(
                opt => Assert.IsNull(opt.IsSet),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

#endregion

#region Store Action

        internal class StoreValue
        {
            [NamedArgument('s', Action = ParseAction.Store)]
            public int Value { get; set; }
        }

        [TestMethod]
        public void Argument_WithStoreAction_StoresValue()
        {
            var result = CliParser.Parse<StoreValue>("-s 10".Split());
            result.Handle(
                opt => Assert.AreEqual(10, opt.Value),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        [TestMethod]
        public void Argument_WithStoreActionAndInconvertibleValue_ThrowsException()
        {
            var result = CliParser.Parse<StoreValue>("-s bbb".Split());
            result.Handle(
                opt => Assert.Fail("Parse succeeded but error was expected."),
                t => Assert.Fail("Trigger initiated, but error was expected."),
                errs => Assert.IsTrue(errs
                    .OfType<ParseException>()
                    .Any()));
        }

        internal class StoreBoolActionNullable
        {
            [NamedArgument('b')]
            public bool? IsSet { get; set; }
        }

        [TestMethod]
        public void Argument_WithStoreBoolActionAndNoValue_StoresBoolAsNull()
        {
            var result = CliParser.Parse<StoreBoolActionNullable>("-b".Split());
            result.Handle(
                opt => Assert.Fail("Parse succeeded but error was expected."),
                t => Assert.Fail("Trigger initiated, but error was expected."),
                errs => Assert.IsTrue(errs
                    .OfType<ParseException>()
                    .Any()));
        }

        [TestMethod]
        public void Argument_WithStoreBoolActionAndNoValue_StoresBoolAsTrue()
        {
            var result = CliParser.Parse<StoreBoolActionNullable>("-b true".Split());
            result.Handle(
                opt => Assert.IsTrue(opt.IsSet.Value),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        [TestMethod]
        public void Argument_WithStoreBoolActionAndNoValue_StoresBoolAsFalse()
        {
            var result = CliParser.Parse<StoreBoolActionNullable>("-b false".Split());
            result.Handle(
                opt => Assert.IsFalse(opt.IsSet.Value),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

#endregion

#region StoreConst Action

        internal class StoreConst
        {
            [NamedArgument('s', Action = ParseAction.StoreConst, Const = 10)]
            public int Value { get; set; }
        }

        [TestMethod]
        public void Argument_WithStoreConstValue_StoresValue()
        {
            var result = CliParser.Parse<StoreConst>("-s".Split());
            result.Handle(
                opt => Assert.AreEqual(10, opt.Value),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        internal class StoreConstWrongConstValue
        {
            [NamedArgument('s', Action = ParseAction.StoreConst, Const = "b")]
            public int Value { get; set; }
        }

        [TestMethod]
        public void Argument_WithStoreConstValueAndInconvertibleConst_ThrowsException()
        {
            var parser = new CliParser<StoreConstWrongConstValue>();
            var errs = parser.ValidateAttributeConfig();

            Assert.IsTrue(errs
                .OfType<ArgumentIntegrityException>()
                .Any());
        }

#endregion

#region Append Action

        internal class Append
        {
            [NamedArgument('i', Action = ParseAction.Append)]
            public List<int> Values { get; set; }
        }

        [TestMethod]
        public void Append_WithMultipleOfSameArgument_AppendsValues()
        {
            var expected = new List<int> {1, 2, 3};
            var result = CliParser.Parse<Append>("-i1 -i 2 -i 3".Split());
            result.Handle(
                opt => CollectionAssert.AreEqual(expected, opt.Values),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        [TestMethod]
        public void Append_WithOneValueInconvertible_ThrowsException()
        {
            var expected = new List<int> { 1, 2, 3 };
            var result = CliParser.Parse<Append>("-i1 -i oops -i 3".Split());
            result.Handle(
                opt => Assert.Fail("Parse succeeded but error was expected."),
                t => Assert.Fail("Trigger initiated, but error was expected."),
                errs => Assert.IsTrue(errs
                    .OfType<ParseException>()
                    .Any()));
        }

#endregion

#region AppendConst Action

        internal class AppendConst
        {
            [NamedArgument('i', Action = ParseAction.AppendConst, Const = 6)]
            public List<int> Values { get; set; }
        }

        [TestMethod]
        public void AppendConst_WithMultipleOfSameArgument_AppendsValues()
        {
            var expected = new List<int> { 6, 6, 6 };
            var result = CliParser.Parse<AppendConst>("-i -i -i".Split());
            result.Handle(
                opt => CollectionAssert.AreEqual(expected, opt.Values),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

#endregion
    }
}

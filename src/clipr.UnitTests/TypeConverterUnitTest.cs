using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using clipr.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace clipr.UnitTests
{
    [TestClass]
    public class TypeConverterUnitTest
    {
        public class CustomDateTimeConverter : StringTypeConverter<DateTime>
        {
            private const string Format = "yyyyMMdd";

            public override DateTime ConvertFrom(CultureInfo culture, string value)
            {
                return DateTime.ParseExact(value, Format, CultureInfo.InvariantCulture, DateTimeStyles.None);
            }
            public override bool IsValid(string value)
            {
                DateTime date;
                return DateTime.TryParseExact(value, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
            }
        }

        [ApplicationInfo(Description = "This is a set of options.")]
        public class CustomDateTimeOptions
        {
            [TypeConverter(typeof(CustomDateTimeConverter))]
            [NamedArgument('d')]
            public DateTime Date { get; set; }
        }

        [TestMethod]
        public void CustomTypeConverter_WithPropertyOnBuiltInClass_UsesCustomConverter()
        {
            var result = CliParser.Parse<CustomDateTimeOptions>("-d 20140801".Split());
            result.Handle(
                opt => Assert.AreEqual(new DateTime(2014, 8, 1), opt.Date),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }


        internal class CustomTypeConversion
        {
            [NamedArgument("set")]
            public CustomType Custom { get; set; }

            [TypeConverter(typeof(CustomTypeConverter))]
            public class CustomType
            {
                public int Value { get; set; }
            }

            public class CustomTypeConverter : StringTypeConverter<CustomType>
            {
                public override CustomType ConvertFrom(CultureInfo culture, string value)
                {
                    var converted = int.Parse(value);
                    return new CustomType
                    {
                        Value = converted
                    };
                }

                public override bool IsValid(string value)
                {
                    int converted;
                    return int.TryParse(value, out converted);
                }
            }
        }

        [TestMethod]
        public void CustomTypeConversion_ParseShortArgOnEnum_ConvertsStringArgumentToCustomType()
        {
            var result = CliParser.Parse<CustomTypeConversion>("--set=3".Split());
            result.Handle(
                opt => Assert.AreEqual(3, opt.Custom.Value),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        internal class CustomListInnerTypeConversion
        {
            [NamedArgument("set", Action = ParseAction.Append, Constraint = NumArgsConstraint.AtLeast, NumArgs = 1)]
            public IList<CustomType> CustomList { get; set; }

            [TypeConverter(typeof(CustomListInnerTypeConverter))]
            public class CustomType
            {
                public int Value { get; set; }
            }

            public class CustomListInnerTypeConverter : StringTypeConverter<CustomType>
            {
                public override CustomType ConvertFrom(CultureInfo culture, string value)
                {
                    var converted = int.Parse(value);
                    return new CustomType
                    {
                        Value = converted
                    };
                }

                public override bool IsValid(string value)
                {
                    int converted;
                    return int.TryParse(value, out converted);
                }
            }
        }

        [TestMethod]
        public void CustomTypeConversion_ParseIntegerList_ConvertsStringArgumentToCustomType()
        {
            var expected = new[] { 3, 5, 4 };

            var result = CliParser.Parse<CustomListInnerTypeConversion>("--set 3 5 4".Split());
            result.Handle(
                opt =>
                {
                    var actual = opt.CustomList.Select(c => c.Value).ToArray();
                    CollectionAssert.AreEqual(expected, actual);
                },
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        internal class ListTypeConversion
        {
            [NamedArgument("numbers", NumArgs = 2)]
            public List<int> Numbers { get; set; }
        }

        [TestMethod]
        public void ListTypeConversion_GivenMultipleIntegers_ConvertsAndAddsToList()
        {
            var expected = new List<int> { 1, 2 };
            var result = CliParser.Parse<ListTypeConversion>("--numbers 1 2".Split());
            result.Handle(
                opt => CollectionAssert.AreEqual(expected, opt.Numbers),
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
        }

        public enum Color
        {
            Red,
            Green,
            Blue
        }

        [StaticEnumeration]
        internal abstract class ColorEnum
        {
            public static readonly ColorEnum Red = new EnumValue(Color.Red);

            public static readonly ColorEnum Blue = new EnumValue(Color.Blue);

            public Color Value { get; private set; }


            public class EnumValue : ColorEnum
            {
                public EnumValue(Color value)
                {
                    Value = value;
                }
            }
        }

        internal class StaticEnumListOptions
        {
            [NamedArgument('c', "colors", Action = ParseAction.Append, Constraint = NumArgsConstraint.AtLeast, Const = 1)]
            public IList<ColorEnum> Colors { get; set; }
        }

        [TestMethod]
        public void ListTypeConversion_GivenStaticEnumList_ConvertsAndAddsToList()
        {
            var expected = new[] { Color.Blue, Color.Red };

            var result = CliParser.Parse<StaticEnumListOptions>("-c Blue Red".Split());
            result.Handle(
                opt =>
                {
                    var actual = opt.Colors.Select(c => c.Value).ToArray();
                    CollectionAssert.AreEqual(expected, actual);
                },
                t => Assert.Fail("Trigger {0} executed.", t),
                e => Assert.Fail("Error parsing arguments."));
            
        }
    }
}

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
            var opt = CliParser.Parse<CustomDateTimeOptions>("-d 20140801".Split());
            Assert.AreEqual(new DateTime(2014, 8, 1), opt.Date);
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
            var opt = CliParser.Parse<CustomTypeConversion>("--set=3".Split());
            Assert.AreEqual(3, opt.Custom.Value);
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

            var opt = CliParser.Parse<CustomListInnerTypeConversion>("--set 3 5 4".Split());
            var actual = opt.CustomList.Select(c => c.Value).ToArray();

            CollectionAssert.AreEqual(expected, actual);
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
            var opt = CliParser.Parse<ListTypeConversion>("--numbers 1 2".Split());
            CollectionAssert.AreEqual(expected, opt.Numbers);
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

            var options = CliParser.Parse<StaticEnumListOptions>("-c Blue Red".Split());
            var actual = options.Colors.Select(c => c.Value).ToArray();

            CollectionAssert.AreEqual(expected, actual);
        }
    }
}

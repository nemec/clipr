using System;
using System.Collections.Generic;

namespace clipr.Sample
{
    class StaticEnums
    {
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


            public class EnumValue : ColorEnum
            {
                public EnumValue(Color value)
                {
                    Value = value;
                }

                public Color Value { get; private set; }

                public override string ToString()
                {
                    return Value.ToString();
                }
            }
        }

        internal class StaticEnumListOptions
        {
            [NamedArgument('c', "colors", Action = ParseAction.Append)]
            public IList<ColorEnum> Colors { get; set; }
        }

        public static void ParseStaticEnumList()
        {
            var result = CliParser.Parse<StaticEnumListOptions>("-c Red -c blue".Split());
            result.Handle(
                opt =>
                {
                    foreach (var color in opt.Colors)
                        Console.WriteLine("Color: {0}", color);
                },
                t => { },
                e => { });
        }
    }
}
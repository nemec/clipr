using System;
using System.Collections.Generic;
using System.Globalization;
using clipr.Utils;

namespace clipr.Sample
{
    public class CustomConverterDateTime
    {
        public class CustomDateTimeConverter : StringTypeConverter<DateTime>
        {
            private static readonly string[] Formats = new[]{ "yyyyMMdd", "yyyy/MM/dd", "yyyy-MM-dd" };

            public override DateTime ConvertFrom(System.Globalization.CultureInfo culture, string value)
            {
                return DateTime.ParseExact(value, Formats, CultureInfo.InvariantCulture, DateTimeStyles.None);
            }
            public override bool IsValid(string value)
            {
                DateTime date;
                return DateTime.TryParseExact(value, Formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
            }
        }

        [ApplicationInfo(Description = "This is a set of options.")]
        public class CustomDateTimeOptions
        {
            [System.ComponentModel.TypeConverter(typeof(CustomDateTimeConverter))]
            [NamedArgument('d', "date", Action = ParseAction.Append,
                           Description = "Store the date.")]
            public List<DateTime> CurrentDate { get; set; }
        }

        public static void Main(string[] args)
        {
            var result = CliParser.Parse<CustomDateTimeOptions>(args);
            result.Handle(
                opt => Console.WriteLine(opt.CurrentDate[1]),
                t => { },
                e => { });
            // >>> 
        }
    }
}
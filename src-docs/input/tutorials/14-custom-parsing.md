Title: Custom Value Parsing
Description: How to customize the way input values are serialized to objects.
---

Custom types may be used as config values, but only if a 
[TypeConverter](http://msdn.microsoft.com/en-us/library/ayybcxe5.aspx) is
defined for that type. Since TypeConverters are big, messy beasts, and
for purposes of this library there is only one conversion case
(string -> MyType), the `clipr.Utils` namespace contains a
`StringTypeConverter` that simplifies the conversion interface. After
implementing the converter, tag your custom class with a
[TypeConverterAttribute](http://msdn.microsoft.com/en-us/library/system.componentmodel.typeconverterattribute.aspx)
to register the converter with .Net.

You may also attach a TypeConverter attribute to a Property on the class,
which allows targeted conversion. It also allows you to apply custom
converters to types that you do not own, like built-in classes.

Below is an example showing how to customize the parsing of a built-in
class, `DateTime`:

```csharp
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
```
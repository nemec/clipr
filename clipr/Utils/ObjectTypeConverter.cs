using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace clipr.Utils
{
    /// <summary>
    /// <para>
    /// A TypeConverter that allows conversion of any object to System.Object.
    /// Does not actually perform a conversion, just passes the value on
    /// through.
    /// </para>
    /// <para>
    /// Must be registered before parsing.
    /// </para>
    /// </summary>
    internal class ObjectTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(String) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return value is string 
                ? value 
                : base.ConvertFrom(context, culture, value);
        }

        public override bool IsValid(ITypeDescriptorContext context, object value)
        {
            return value is string || base.IsValid(context, value);
        }

        public static void Register()
        {
            var attr = new Attribute[]
                {
                    new TypeConverterAttribute(typeof(ObjectTypeConverter)) 
                };
            TypeDescriptor.AddAttributes(typeof(object), attr);
        }
    }
}

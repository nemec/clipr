using System;
using System.ComponentModel;
using System.Globalization;

namespace clipr
{
    /// <summary>
    /// Provides a simpler abstraction to the <see cref="TypeConverter"/>
    /// interface allowing users to specify custom conversion methods
    /// between the command line arguments (as strings) and their
    /// custom type. This is solely a convenience class, normal
    /// <see cref="TypeConverter"/>s will be handled correctly as well.
    /// </summary>
    /// <typeparam name="T">Custom destination type.</typeparam>
    public abstract class StringTypeConverter<T> : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof (String) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var s = value as string;
            return s != null ? 
                ConvertFrom(culture, s) : 
                base.ConvertFrom(context, culture, value);
        }

        public abstract T ConvertFrom(CultureInfo culture, string value); 

        public override bool IsValid(ITypeDescriptorContext context, object value)
        {
            var s = value as string;
            return s != null ? IsValid(s) : base.IsValid(context, value);
        }

        public abstract bool IsValid(string value);
    }
}

using System;
using System.ComponentModel;
using System.Globalization;

namespace clipr.Utils
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
        /// <inheritdoc/>
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return typeof(T) == destinationType;
        }

        /// <inheritdoc/>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof (String) || base.CanConvertFrom(context, sourceType);
        }

        /// <inheritdoc/>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var s = value as string;
            return s != null ? 
                ConvertFrom(culture, s) : 
                base.ConvertFrom(context, culture, value);
        }

        /// <summary>
        /// Convert the string value to an instance of
        /// <typeparamref name="T"/>.
        /// </summary>
        /// <param name="culture">
        /// The <see cref="T:System.Globalization.CultureInfo"/> to use as
        /// the current culture.
        /// </param>
        /// <param name="value">String value to transform.</param>
        /// <returns></returns>
        public abstract T ConvertFrom(CultureInfo culture, string value);

        /// <inheritdoc/>
        public override bool IsValid(ITypeDescriptorContext context, object value)
        {
            var s = value as string;
            return s != null ? IsValid(s) : base.IsValid(context, value);
        }

        /// <summary>
        /// Identify whether or not the string can be converted to
        /// <typeparamref name="T" />.
        /// </summary>
        /// <param name="value">String value to transform.</param>
        /// <returns>True if string is convertible.</returns>
        public abstract bool IsValid(string value);
    }
}

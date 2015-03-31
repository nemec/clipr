using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace clipr.Utils
{
    internal class StaticEnumerationConverter : TypeConverter
    {
        private readonly Type _enumType;

        private readonly Dictionary<string, FieldInfo> _fields;

        public StaticEnumerationConverter(Type enumType, Dictionary<string, FieldInfo> fields)
        {
            _enumType = enumType;
            _fields = fields;
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return _enumType == destinationType;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof (String) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var s = value as string;
            FieldInfo field;

            if (s == null || !_fields.TryGetValue(s, out field))
            {
                throw new InvalidCastException(String.Format(
                    "'{0}' is not a valid value for enum '{1}'",
                    s, _enumType.Name));
            }

            return field.GetValue(null);  // static field
        }

        public override bool IsValid(ITypeDescriptorContext context, object value)
        {
            var s = value as string;
            return s != null && _fields.ContainsKey(s);
        }
    }
}

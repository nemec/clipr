using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace clipr
{
    internal static class PropertyExtensions
    {
        public static object GetDefaultValue(this PropertyInfo prop)
        {
            // From http://stackoverflow.com/a/2688165/564755
            var t = prop.PropertyType;
            if (t.IsValueType && Nullable.GetUnderlyingType(t) == null)
            {
                return Activator.CreateInstance(t);
            }
            return null;
        }

        public static bool IsValidIList(this PropertyInfo prop)
        {
            return prop.PropertyType.GetInterfaces().Any(
                t => t.IsGenericType &&
                     t.GetGenericTypeDefinition() == typeof (IList<>));
        }

        public static bool IsValid<T>(this PropertyInfo prop)
        {
            return prop.PropertyType == typeof(T) ||
                TypeDescriptor.GetConverter(prop.PropertyType)
                .CanConvertTo(typeof(T));
        }

        public static bool ValueIsConvertible(this PropertyInfo prop, object obj)
        {
            return obj == null || prop.PropertyType == obj.GetType() ||
                TypeDescriptor.GetConverter(prop.PropertyType)
                          .IsValid(obj);
        }

        public static bool ValueIsConvertibleGeneric(this PropertyInfo prop, object obj)
        {
            return obj == null ||
                prop.PropertyType.GenericTypeArguments.First() == obj.GetType() ||
                TypeDescriptor.GetConverter(prop.PropertyType).IsValid(obj);
        }
    }
}

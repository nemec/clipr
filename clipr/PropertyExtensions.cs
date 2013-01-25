using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using clipr.Annotations;

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

        internal static bool IsValidIList(this PropertyInfo prop)
        {
            return prop.PropertyType.GetInterfaces().Any(
                t => t.IsGenericType &&
                     t.GetGenericTypeDefinition() == typeof (IList<>));
        }

        internal static bool IsValid<T>(this PropertyInfo prop)
        {
            return prop.PropertyType == typeof(T) ||
                TypeDescriptor.GetConverter(prop.PropertyType)
                .CanConvertTo(typeof(T));
        }

        internal static bool ValueIsConvertible(this PropertyInfo prop, object obj)
        {
            return obj == null || prop.PropertyType == obj.GetType() ||
                TypeDescriptor.GetConverter(prop.PropertyType)
                          .IsValid(obj);
        }

        internal static bool ValueIsConvertibleGeneric(this PropertyInfo prop, object obj)
        {
            return obj == null ||
                prop.PropertyType.GetGenericArguments().First() == obj.GetType() ||
                TypeDescriptor.GetConverter(prop.PropertyType).IsValid(obj);
        }

        internal static string[] GetMutuallyExclusiveGroups(this PropertyInfo prop)
        {
            return prop.GetCustomAttributes<MutuallyExclusiveGroupAttribute>()
                .Select(a => a.Name)
                .ToArray();
        }
    }
}

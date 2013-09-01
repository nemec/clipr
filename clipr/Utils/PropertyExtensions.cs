using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace clipr.Utils
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

        internal static bool IsValidIEnumerable(this PropertyInfo prop)
        {
            var enumT = typeof (IEnumerable<>);
            var type = prop.PropertyType;

            return type.IsGenericType && (
                type.GetGenericTypeDefinition() == enumT ||
                type.GetInterfaces().Any(t => t.GetGenericTypeDefinition()  == enumT));
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

        internal static List<string> GetMutuallyExclusiveGroups(this PropertyInfo prop)
        {
            return prop.GetCustomAttributes<MutuallyExclusiveGroupAttribute>()
                .Select(a => a.Name)
                .ToList();
        }
    }
}

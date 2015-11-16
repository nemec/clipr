using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace clipr.Utils
{
    internal static class PropertyExtensions
    {
        public static object GetDefaultValue(this Type t)
        {
            // From http://stackoverflow.com/a/2688165/564755
            if (t.IsValueType && Nullable.GetUnderlyingType(t) == null)
            {
                return Activator.CreateInstance(t);
            }
            return null;
        }

        internal static bool IsValidEnumerable(this Type type)
        {
            var enumT = typeof(IEnumerable<>);
            return type.IsGenericType && (
                type.GetGenericTypeDefinition() == enumT ||
                type.GetInterfaces().Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == enumT));
        }

        internal static bool IsValid<T>(this Type t)
        {
            return t == typeof(T) ||
                TypeDescriptor.GetConverter(t)
                .CanConvertTo(typeof(T));
        }

        internal static bool ValueIsConvertible(this Type t, object obj)
        {
            return obj == null || t == obj.GetType() ||
                TypeDescriptor.GetConverter(t)
                          .IsValid(obj);
        }

        internal static bool ValueIsConvertibleGeneric(this Type t, object obj)
        {
            return obj == null ||
                t.GetGenericArguments().First() == obj.GetType() ||
                TypeDescriptor.GetConverter(t).IsValid(obj);
        }

        internal static List<string> GetMutuallyExclusiveGroups(this PropertyInfo prop)
        {
            return prop.GetCustomAttributes<MutuallyExclusiveGroupAttribute>()
                .Select(a => a.Name)
                .ToList();
        }
    }
}

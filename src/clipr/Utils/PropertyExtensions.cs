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
            if (t.GetTypeInfo().IsValueType && Nullable.GetUnderlyingType(t) == null)
            {
                return Activator.CreateInstance(t);
            }
            return null;
        }

        internal static bool IsValidEnumerable(this Type type)
        {
            var enumT = typeof(IEnumerable<>);
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsGenericType && (
                typeInfo.GetGenericTypeDefinition() == enumT ||
                typeInfo.GetInterfaces().Any(t => 
                    t.GetTypeInfo().IsGenericType && 
                    t.GetTypeInfo().GetGenericTypeDefinition() == enumT));
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
                t.GetTypeInfo().GetGenericArguments().First() == obj.GetType() ||
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

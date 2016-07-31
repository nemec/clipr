using clipr.Core;
using System;
using System.Linq;
using System.Reflection;

namespace clipr.Utils
{
    public static class EnumUtils
    {
        public static bool IsEnum(IValueStoreDefinition store)
        {
            return IsClrEnum(store) || IsStaticEnum(store);
        }

        private static bool IsClrEnum(IValueStoreDefinition store)
        {
            return store.Type.GetTypeInfo().IsSubclassOf(typeof(Enum));
        }

        private static bool IsStaticEnum(IValueStoreDefinition store)
        {
            return (store.GetCustomAttribute<StaticEnumerationAttribute>() ??
                    store.Type.GetTypeInfo().GetCustomAttribute<StaticEnumerationAttribute>()) != null;
        }

        public static string[] GetEnumValues(IValueStoreDefinition store)
        {
            var typeInfo = store.Type.GetTypeInfo();
            if (IsClrEnum(store))
            {
                return Enum.GetNames(store.Type);
            }
            else if (IsStaticEnum(store))
            {
                return typeInfo.GetFields(
                    BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.IsInitOnly &&
                            typeInfo.IsAssignableFrom(f.FieldType))
                .Select(f => f.Name)
                .ToArray();
            }
            return new string[0];
        }
    }
}

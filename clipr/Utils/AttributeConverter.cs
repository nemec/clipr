using System.Collections.Generic;
using System.Reflection;
using clipr.Arguments;
using clipr.Core;
using System.ComponentModel;
using System.Linq;
using System;

namespace clipr.Utils
{
    internal static class AttributeConverter
    {
        private static TypeConverter[] GetConverters(PropertyInfo prop)
        {
            var staticEnumConverter = GetStaticEnumerationConverter(prop);

            var types = prop.GetCustomAttributes<TypeConverterAttribute>()
                .Select(attr => Type.GetType(attr.ConverterTypeName))
                .Where(t => t != null)
                .Select(Activator.CreateInstance)
                .ToList();
            return staticEnumConverter
                .Concat(types.OfType<TypeConverter>())
                .ToArray();
        }

        private static IEnumerable<TypeConverter> GetStaticEnumerationConverter(PropertyInfo prop)
        {
            var attr = prop.GetCustomAttribute<StaticEnumerationAttribute>()
                ?? prop.PropertyType.GetCustomAttribute<StaticEnumerationAttribute>();
            if (attr == null)
            {
                return Enumerable.Empty<TypeConverter>();
            }

            var type = prop.PropertyType;
            var fields = type.GetFields(
                BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.IsInitOnly &&  // readonly
                            type.IsAssignableFrom(f.FieldType))  // same type as class, or subclass
                .ToDictionary(k => k.Name, StringComparer.InvariantCultureIgnoreCase);
            if (!fields.Any())
            {
                return Enumerable.Empty<TypeConverter>();
            }

            return new TypeConverter[]
            {
                new StaticEnumerationConverter(type, fields)
            };
        }

        private static void SetDefaults(ArgumentAttribute attr)
        {
            // If appending and didn't explicitly set the constraint,
            // the sane default is to accept AtLeast N parameters.
            if(!attr.ExplicitlySetConstraint && 
               (attr.Action == ParseAction.Append || attr.Action == ParseAction.AppendConst))
            {
                attr.Constraint = NumArgsConstraint.AtLeast;
            }
        }

        public static INamedArgument ToNamedArgument(this PropertyInfo prop)
        {
            var attr = prop.GetCustomAttribute<NamedArgumentAttribute>();
            attr.MutuallyExclusiveGroups = prop.GetMutuallyExclusiveGroups();
            attr.Name = prop.Name;
            attr.Store = new PropertyValueStore(prop, GetConverters(prop));
            SetDefaults(attr);
            return attr;
        }

        public static IPositionalArgument ToPositionalArgument(this PropertyInfo prop)
        {
            var attr = prop.GetCustomAttribute<PositionalArgumentAttribute>();
            attr.Name = prop.Name;
            attr.Store = new PropertyValueStore(prop, GetConverters(prop));
            SetDefaults(attr);
            return attr;
        }
    }
}

using System.Collections.Generic;
using System.Reflection;
using clipr.Arguments;
using clipr.Core;
using System.ComponentModel;
using System.Linq;
using System;
using clipr.Attributes;

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
                .Select(Activator.CreateInstance);
            return staticEnumConverter
                .Concat(types.OfType<TypeConverter>())
                .ToArray();
        }

        public static TypeConverter[] GetConverters(Type type)
        {
            var staticEnumConverter = GetStaticEnumerationConverter(type);
            var typeInfo = type.GetTypeInfo();
            var types = typeInfo.GetCustomAttributes<TypeConverterAttribute>()
                .Select(attr => Type.GetType(attr.ConverterTypeName))
                .Where(t => t != null)
                .Select(Activator.CreateInstance);

            return staticEnumConverter
                .Concat(types.OfType<TypeConverter>())
                .ToArray();
        }

        private static IEnumerable<TypeConverter> GetStaticEnumerationConverter(PropertyInfo prop)
        {
            var type = prop.PropertyType;
            var typeInfo = type.GetTypeInfo();
            var attr = prop.GetCustomAttribute<StaticEnumerationAttribute>()
                ?? typeInfo.GetCustomAttribute<StaticEnumerationAttribute>();
            if (attr == null)
            {
                return Enumerable.Empty<TypeConverter>();
            }
            return GetStaticEnumerationConverterGated(type);
        }

        private static IEnumerable<TypeConverter> GetStaticEnumerationConverter(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            if (typeInfo.GetCustomAttribute<StaticEnumerationAttribute>() == null)
            {
                return Enumerable.Empty<TypeConverter>();
            }
            return GetStaticEnumerationConverterGated(type);
        }

        // Call this once we've determined the type has an attribute tagged to it
        private static IEnumerable<TypeConverter> GetStaticEnumerationConverterGated(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            var fields = typeInfo.GetFields(
                BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.IsInitOnly &&  // readonly
                            typeInfo.IsAssignableFrom(f.FieldType))  // same type as class, or subclass
                .ToDictionary(k => k.Name,
#if NET35
                    StringComparer.InvariantCultureIgnoreCase);
#else
                    StringComparer.OrdinalIgnoreCase);
#endif
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

            // TODO evaluate setting "StoreTrue" as the default for boolean arguments
        }

        public static INamedArgument ToNamedArgument(this PropertyInfo prop)
        {
            var attr = prop.GetCustomAttribute<NamedArgumentAttribute>();
            attr.MutuallyExclusiveGroups = prop.GetMutuallyExclusiveGroups();
            attr.Name = prop.Name;
            attr.LocalizationInfo = GetLocalizationInfo(prop);

            var prompt = prop.GetCustomAttribute<PromptIfValueMissingAttribute>();
            attr.PromptIfValueMissing = new PromptIfValueMissing();
            if(prompt != null)
            {
                attr.PromptIfValueMissing.Enabled = true;
                attr.PromptIfValueMissing.MaskInput = prompt.MaskInput;
            }

            attr.Store = new PropertyValueStore(prop, GetConverters(prop));
            SetDefaults(attr);
            return attr;
        }

        public static IPositionalArgument ToPositionalArgument(this PropertyInfo prop)
        {
            var attr = prop.GetCustomAttribute<PositionalArgumentAttribute>();
            attr.Name = prop.Name;
            attr.LocalizationInfo = GetLocalizationInfo(prop);
            attr.PromptIfValueMissing = new PromptIfValueMissing();
            attr.Store = new PropertyValueStore(prop, GetConverters(prop));
            SetDefaults(attr);
            return attr;
        }

        private static LocalizationInfo GetLocalizationInfo(PropertyInfo prop)
        {
            var attr = prop.GetCustomAttribute<LocalizeAttribute>();
            if(attr == null)
            {
                return null;
            }
            var info = new LocalizationInfo
            {
                ResourceName = attr.ResourceName ?? (prop.DeclaringType.Name + "_" + prop.Name)
            };

            if(info.ResourceType != null)
            {
                info.ResourceType = attr.ResourceType;
            }
            else
            {
                var parent = prop.DeclaringType.GetTypeInfo().GetCustomAttribute<LocalizeAttribute>();
                if(parent == null || parent.ResourceType == null)
                {
                    throw new ArgumentException(String.Format(
                        "The property '{0}' or its parent class '{1}' must define a ResourceType in order to be localized.",
                        prop.Name, prop.DeclaringType.Name));
                }
                info.ResourceType = parent.ResourceType;
            }

            return info;
        }

        private static LocalizationInfo GetLocalizationInfo(Type type)
        {
            var attr = type.GetTypeInfo().GetCustomAttribute<LocalizeAttribute>();
            if (attr == null || attr.ResourceType == null)
            {
                throw new ArgumentException(String.Format(
                    "The class '{0}' must define a ResourceType in order to be localized.",
                    type.Name));
            }
            var info = new LocalizationInfo
            {
                ResourceType = attr.ResourceType,
                ResourceName = attr.ResourceName ?? type.Name
            };

            return info;
        }
    }
}

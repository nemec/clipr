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
                    StringComparer.OrdinalIgnoreCase);
            if (!fields.Any())
            {
                return Enumerable.Empty<TypeConverter>();
            }

            return new TypeConverter[]
            {
                new StaticEnumerationConverter(type, fields)
            };
        }
        
        private static readonly Lazy<Type[]> _listBases = new Lazy<Type[]>(() => 
                    typeof(IList<>)
                        .GetInterfaces()
                        .Where(i => i.IsGenericType)
                        .Select(i => i.GetGenericTypeDefinition()).ToArray());

        private static void SetDefaults(ArgumentAttribute attr)
        {
            if(!attr.ExplicitlySetConstraint)
            {
                // If appending and didn't explicitly set the constraint,
                // the sane default is to accept AtLeast N parameters.
                if(attr.Action == ParseAction.Append || attr.Action == ParseAction.AppendConst)
                {
                    attr.Constraint = NumArgsConstraint.AtLeast;
                }

                // If IList is assignable to property and didn't explicitly set the constraint,
                // the sane default is to accept AtLeast N parameters.
                var typ = attr.Store.Type;
                if(typ.IsGenericType)
                {
                    var gen = typ.GetGenericTypeDefinition();
                    if(typeof(IList<>) == gen || _listBases.Value.Any(i => i == gen))
                    {
                        attr.Constraint = NumArgsConstraint.AtLeast;
                    }
                }
            }
            
            if(!attr.ExplicitlySetAction)
            {
                // Set "StoreTrue" as the default action for
                // boolean arguments if not explicitly set.
                if(attr.Store.Type == typeof(bool))
                {
                    attr.Action = ParseAction.StoreTrue;
                }
            }
        }

        public static INamedArgument ToNamedArgument(this PropertyInfo prop, Type defaultResourceType)
        {
            var attr = prop.GetCustomAttribute<NamedArgumentAttribute>();
            attr.Name = prop.Name;
            attr.LocalizationInfo = GetLocalizationInfo(prop, defaultResourceType);

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

        public static IPositionalArgument ToPositionalArgument(this PropertyInfo prop, Type defaultResourceType)
        {
            var attr = prop.GetCustomAttribute<PositionalArgumentAttribute>();
            attr.Name = prop.Name;
            attr.LocalizationInfo = GetLocalizationInfo(prop, defaultResourceType);
            attr.PromptIfValueMissing = new PromptIfValueMissing();
            attr.Store = new PropertyValueStore(prop, GetConverters(prop));
            SetDefaults(attr);
            return attr;
        }

        public static LocalizationInfo GetLocalizationInfo(PropertyInfo prop, Type defaultResourceType)
        {
            var attr = prop.GetCustomAttribute<LocalizeAttribute>();
            if(attr == null)
            {
                return null;
            }
            var info = new LocalizationInfo
            {
                ResourceName = attr.ResourceName ?? String.Format("{0}_{1}", prop.DeclaringType.Name, prop.Name)
            };

            if(info.ResourceType != null)
            {
                info.ResourceType = attr.ResourceType;
            }
            else if(defaultResourceType != null)
            {
                info.ResourceType = defaultResourceType;
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

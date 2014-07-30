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
            var attrs = prop.GetCustomAttributes<TypeConverterAttribute>().ToList();
            var types = attrs.Select(a => Activator.CreateInstance(Type.GetType(a.ConverterTypeName))).ToList();
            return types.OfType<TypeConverter>().ToArray();
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
            SetDefaults(attr);
            attr.MutuallyExclusiveGroups = prop.GetMutuallyExclusiveGroups();
            attr.Name = prop.Name;
            attr.Store = new PropertyValueStore(prop, GetConverters(prop));
            return attr;
        }

        public static IPositionalArgument ToPositionalArgument(this PropertyInfo prop)
        {
            var attr = prop.GetCustomAttribute<PositionalArgumentAttribute>();
            SetDefaults(attr);
            attr.Name = prop.Name;
            attr.Store = new PropertyValueStore(prop, GetConverters(prop));
            return attr;
        }
    }
}

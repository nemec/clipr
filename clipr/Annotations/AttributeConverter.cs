using System.Reflection;
using clipr.Arguments;

namespace clipr.Annotations
{
    internal static class AttributeConverter
    {
        public static INamedArgument ToNamedArgument(this PropertyInfo prop)
        {
            var attr = prop.GetCustomAttribute<NamedArgumentAttribute>();
            attr.MutuallyExclusiveGroups = prop.GetMutuallyExclusiveGroups();
            attr.ArgumentName = prop.Name;
            attr.Property = prop;
            return attr;
        }

        public static IPositionalArgument ToPositionalArgument(this PropertyInfo prop)
        {
            var attr = prop.GetCustomAttribute<PositionalArgumentAttribute>();
            attr.ArgumentName = prop.Name;
            attr.Property = prop;
            return attr;
        }
    }
}

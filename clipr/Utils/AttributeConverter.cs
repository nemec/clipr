using System.Reflection;
using clipr.Arguments;
using clipr.Core;

namespace clipr.Utils
{
    internal static class AttributeConverter
    {
        public static INamedArgument ToNamedArgument(this PropertyInfo prop)
        {
            var attr = prop.GetCustomAttribute<NamedArgumentAttribute>();
            attr.MutuallyExclusiveGroups = prop.GetMutuallyExclusiveGroups();
            attr.Name = prop.Name;
            attr.Store = new PropertyValueStore(prop);
            return attr;
        }

        public static IPositionalArgument ToPositionalArgument(this PropertyInfo prop)
        {
            var attr = prop.GetCustomAttribute<PositionalArgumentAttribute>();
            attr.Name = prop.Name;
            attr.Store = new PropertyValueStore(prop);
            return attr;
        }
    }
}

using System;
using System.Reflection;
using System.ComponentModel;
using clipr.Utils;

namespace clipr.Core
{
    internal class PropertyValueStore : IValueStoreDefinition
    {
        private PropertyInfo Property { get; set; }

        public TypeConverter[] Converters { get; private set; }

        public PropertyValueStore(PropertyInfo prop, TypeConverter[] converters = null)
        {
            Property = prop;
            Converters = converters ?? new TypeConverter[0];
        }

        public string Name { get { return Property.Name; } }

        public void SetValue(object source, object value)
        {
            Property.SetValue(source, value, null);
        }

        public object GetValue(object source)
        {
            return Property.GetValue(source, null);
        }

        public TAttribute GetCustomAttribute<TAttribute>() where TAttribute : Attribute
        {
            return Property.GetCustomAttribute<TAttribute>();
        }

        public Type Type
        {
            get { return Property.PropertyType; }
        }
    }
}

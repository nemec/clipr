using System;
using System.Reflection;

namespace clipr.Core
{
    internal class PropertyValueStore : IValueStoreDefinition
    {
        private PropertyInfo Property { get; set; }

        public PropertyValueStore(PropertyInfo prop)
        {
            Property = prop;
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

        public Type Type
        {
            get { return Property.PropertyType; }
        }
    }
}

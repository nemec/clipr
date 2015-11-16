using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ComponentModel;

namespace clipr.Core
{
    internal class IndexerValueStore : IValueStoreDefinition
    {
        public IndexerValueStore(string name, object key, MethodInfo getter, MethodInfo setter, TypeConverter[] converters)
        {
            Name = name;
            Getter = getter;
            Setter = setter;
            Key = key;
            Converters = converters ?? new TypeConverter[0];
        }

        private MethodInfo Getter { get; set; }

        private MethodInfo Setter { get; set; }

        private object Key { get; set; }

        public string Name { get; private set; }

        public TypeConverter[] Converters { get; private set; }

        public void SetValue(object parent, object value)
        {
            Setter.Invoke(parent, new[] {Key, value});
        }

        public object GetValue(object parent)
        {
            return Getter.Invoke(parent, null);
        }

        public TAttribute GetCustomAttribute<TAttribute>() where TAttribute : Attribute
        {
            return null;
        }

        public IEnumerable<TAttribute> GetCustomAttributes<TAttribute>() where TAttribute : Attribute
        {
            return Enumerable.Empty<TAttribute>();
        }

        public Type Type
        {
            get { return Getter.ReturnType; }
        }
    }
}

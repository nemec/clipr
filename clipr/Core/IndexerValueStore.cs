using System;
using System.Reflection;

namespace clipr.Core
{
    internal class IndexerValueStore : IValueStoreDefinition
    {
        public IndexerValueStore(string name, object key, MethodInfo getter, MethodInfo setter)
        {
            Name = name;
            Getter = getter;
            Setter = setter;
            Key = key;
        }

        private MethodInfo Getter { get; set; }

        private MethodInfo Setter { get; set; }

        private object Key { get; set; }

        public string Name { get; private set; }

        public void SetValue(object source, object value)
        {
            Setter.Invoke(source, new[] {Key, value});
        }

        public object GetValue(object source)
        {
            return Getter.Invoke(source, null);
        }

        public Type Type
        {
            get { return Getter.ReturnType; }
        }
    }
}

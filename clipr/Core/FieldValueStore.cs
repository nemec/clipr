using System;
using System.Reflection;
using System.ComponentModel;

namespace clipr.Core
{
    internal class FieldValueStore : IValueStoreDefinition
    {
        private FieldInfo Field { get; set; }

        public TypeConverter[] Converters { get; private set; }

        public FieldValueStore(FieldInfo field, TypeConverter[] converters = null)
        {
            Field = field;
            Converters = converters ?? new TypeConverter[0];
        }

        public string Name { get { return Field.Name; } }

        public void SetValue(object source, object value)
        {
            Field.SetValue(source, value);
        }

        public object GetValue(object source)
        {
            return Field.GetValue(source);
        }

        public Type Type
        {
            get { return Field.FieldType; }
        }
    }
}

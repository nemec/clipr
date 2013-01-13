using System;

namespace clipr.Usage
{
    [AttributeUsage(AttributeTargets.Property)]
    class ResourceKeyAttribute : Attribute
    {
        public string Key { get; private set; }

        public ResourceKeyAttribute(string key)
        {
            Key = key;
        }
    }
}

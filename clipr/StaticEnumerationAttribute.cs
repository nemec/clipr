using System;

namespace clipr
{
    /// <summary>
    /// Marks a class as an enumeration with static readonly fields. When
    /// generating usage information, all static readonly fields will be
    /// displayed as options.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public class StaticEnumerationAttribute : Attribute
    {
    }
}

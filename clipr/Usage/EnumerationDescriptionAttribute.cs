using System;

namespace clipr.Usage
{
    /// <summary>
    /// Describes a specific enum value
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class EnumerationDescriptionAttribute : Attribute
    {
        /// <summary>
        /// Text description
        /// </summary>
        public readonly string Description;

        /// <summary>
        /// Create a description for a specific enum value
        /// </summary>
        /// <param name="description"></param>
        public EnumerationDescriptionAttribute(string description)
        {
            Description = description;
        }
    }
}

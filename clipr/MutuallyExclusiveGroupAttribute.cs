using System;

namespace clipr
{
    /// <summary>
    /// <para>
    /// Subscribes the property to a mutually exclusive parsing group.
    /// If multiple command line arguments are provided from one
    /// group, a <see cref="ParseException"/> will be thrown.
    /// </para>
    /// <para>
    /// If any attribute marks a group as required and and
    /// no command line arguments are provided for the group, 
    /// a <see cref="ParseException"/> will also be thrown.
    /// </para>
    /// <para>
    /// This attribute only applies to <see cref="NamedArgumentAttribute"/>s.
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class MutuallyExclusiveGroupAttribute : Attribute
    {
        /// <summary>
        /// Group name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// At least one argument in the group must be provided when parsing.
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// <para>
        /// Subscribes the property to a mutually exclusive parsing group.
        /// If multiple command line arguments are provided from one
        /// group, a <see cref="ParseException"/> will be thrown.
        /// </para>
        /// <para>
        /// If a group is required and no command line arguments are provided
        /// for the group, a <see cref="ParseException"/> will also be thrown.
        /// </para>
        /// <para>
        /// This attribute only applies to <see cref="NamedArgumentAttribute"/>s.
        /// </para>
        /// </summary>
        public MutuallyExclusiveGroupAttribute(string name)
        {
            Name = name;
            Required = false;
        }
    }
}

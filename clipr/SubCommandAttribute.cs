using System;

namespace clipr
{
    /// <summary>
    /// Mark the property as a subcommand. (cf. 'svn checkout')
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class SubCommandAttribute : Attribute
    {
        /// <summary>
        /// Name of the subcommand. If provided as an argument, it
        /// will trigger parsing of the subcommand.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Description of the subcommand, suitable for help pages.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Create a new subcommand.
        /// </summary>
        /// <param name="name"></param>
        public SubCommandAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Create a new subcommand.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        public SubCommandAttribute(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}

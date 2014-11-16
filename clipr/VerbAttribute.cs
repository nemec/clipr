using System;

namespace clipr
{
    /// <summary>
    /// Mark the property as a subcommand. (cf. 'svn checkout')
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class VerbAttribute : Attribute
    {
        /// <summary>
        /// Name of the subcommand. If provided as an argument, it
        /// will trigger parsing of the subcommand.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the subcommand, suitable for help pages.
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Create a new subcommand.
        /// </summary>
        public VerbAttribute()
        {
            
        }

        /// <summary>
        /// Create a new subcommand.
        /// </summary>
        /// <param name="name"></param>
        public VerbAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Create a new subcommand.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        public VerbAttribute(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }
}

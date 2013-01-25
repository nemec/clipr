using System;

namespace clipr.Annotations
{
    /// <summary>
    /// Provides extra information about the set of options.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandAttribute : Attribute
    {
        /// <summary>
        /// Name of the command/program.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// High level description of the command.
        /// </summary>
        public string Description { get; set; }
    }
}

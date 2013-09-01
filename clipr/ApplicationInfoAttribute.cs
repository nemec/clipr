using System;

namespace clipr
{
    /// <summary>
    /// Provides extra information about the main set of arguments.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ApplicationInfoAttribute : Attribute
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

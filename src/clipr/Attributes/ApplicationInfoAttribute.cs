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
        /// Executable name of the command/program.
        /// This is the application name the user
        /// should be entering into their terminal/console.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// High level description of the command.
        /// </summary>
        public string Description { get; set; }
    }
}

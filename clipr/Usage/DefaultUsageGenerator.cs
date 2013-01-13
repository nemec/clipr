using System;
using System.IO;
using System.Reflection;

namespace clipr.Usage
{
    /// <summary>
    /// Builds usage information automatically from the associated
    /// type.
    /// </summary>
    /// <typeparam name="T">Type to inspect.</typeparam>
    public class DefaultUsageGenerator<T> : IUsageGenerator
    {
        /// <summary>
        /// Name of the program being executed.
        /// </summary>
        public string ProgramName { get; set; }

        public char? ShortName { get; set; }

        public string LongName { get; set; }

        private string Description { get; set; }

        public IVersion Version { get; set; }

        /// <summary>
        /// Create a new generator with no description and the
        /// program name from the loaded executable.
        /// </summary>
        public DefaultUsageGenerator()
            : this(null)
        {
        } 

        /// <summary>
        /// Create a new generator with the given description and the
        /// program name from the loaded executable.
        /// </summary>
        /// <param name="description">General description for the help page.</param>
        public DefaultUsageGenerator(string description)
            : this(description,
            Path.GetFileName(Assembly.GetExecutingAssembly().CodeBase))
        {
        }

        /// <summary>
        /// Create a new generator with the given description and the
        /// given program name.
        /// </summary>
        /// <param name="description">General description for the help page.</param>
        /// <param name="programName">Name of the program being run.</param>
        public DefaultUsageGenerator(string description, string programName)
        {
            Description = description;
            ProgramName = programName;
            ShortName = 'h';
            LongName = "help";
            Version = new ExecutingAssemblyVersion();
        } 

        public string GetUsage()
        {
            return String.Format("USAGE: {0} someargs\n\n{1}",
                ProgramName, Description ?? "No description.");
        }

        // TODO additional argument groups (aside from named and positional)?
    }
}

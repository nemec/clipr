using System;

namespace clipr.Usage
{
    /// <summary>
    /// Generate usage information from localizable resource files.
    /// </summary>
    public class ResourceUsageGenerator : IUsageGenerator
    {
        public char? ShortName { get; set; }

        public string LongName { get; set; }

        public IVersion Version { get; set; }

        /// <summary>
        /// Create a new usage generator with the
        /// default argument names (-h and --help).
        /// </summary>
        public ResourceUsageGenerator()
        {
            ShortName = 'h';
            LongName = "help";
        }

        public string GetUsage()
        {
            throw new NotImplementedException();
        }
    }
}

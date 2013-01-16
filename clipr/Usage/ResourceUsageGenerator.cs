using System;

namespace clipr.Usage
{
    /// <summary>
    /// Generate usage information from localizable resource files.
    /// </summary>
    public class ResourceUsageGenerator<T> : IHelpGenerator<T> where T : class
    {
        public ParserConfig<T> Config { get; set; } 

        public char? ShortName { get; set; }

        public string LongName { get; set; }

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


        public string GetHelp()
        {
            throw new NotImplementedException();
        }

        public string PluginName
        {
            get { return "ResourceUsageGenerator"; }
        }

        public void OnParse()
        {
            throw new NotImplementedException();
        }
    }
}

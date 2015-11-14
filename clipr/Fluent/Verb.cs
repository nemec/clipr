
namespace clipr.Fluent
{
    public class Verb<TConf> where TConf : class 
    {
        internal Verb(CliParserBuilder<TConf> parser)
        {
            And = parser;
        }

        /// <summary>
        /// Return to the parser.
        /// </summary>
        public CliParserBuilder<TConf> And { get; set; }
    }
}

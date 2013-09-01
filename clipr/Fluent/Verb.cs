
namespace clipr.Fluent
{
    public class Verb<TConf> where TConf : class 
    {
        internal Verb(CliParser<TConf> parser)
        {
            And = parser;
        }

        /// <summary>
        /// Return to the parser.
        /// </summary>
        public CliParser<TConf> And { get; set; }
    }
}

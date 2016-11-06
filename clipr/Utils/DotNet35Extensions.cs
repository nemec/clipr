using System;
using System.Collections.Generic;
using System.Linq;

namespace clipr.Utils
{
    /// <summary>
    /// An exception that aggregates multiple other exceptions together.
    /// </summary>
    public class AggregateException : Exception
    {
        private readonly IEnumerable<Exception> _exceptions;

        /// <summary>
        /// Create a new AggregateException instance that contains the
        /// given exceptions.
        /// </summary>
        /// <param name="exceptions"></param>
// ReSharper disable PossibleMultipleEnumeration
        public AggregateException(IEnumerable<Exception> exceptions)
            : base("", exceptions.First())
        {
            _exceptions = exceptions;
        }
// ReSharper restore PossibleMultipleEnumeration

        /// <summary>
        /// Handle each exception.
        /// </summary>
        /// <param name="handler"></param>
        public void Handle(Func<Exception, bool> handler)
        {
            foreach (var exception in _exceptions)
            {
                if (!handler(exception))
                {
                    throw exception;
                }
            }
        }
    }

#if NET35
    /// <summary>
    /// Extensions to convert .Net3.5-compatible code to the .Net4.5 syntax.
    /// </summary>
    public static class DotNet35Extensions
    {
        /// <summary>
        /// Checks whether or not the enum has the given flag set.
        /// </summary>
        /// <param name="en"></param>
        /// <param name="cmp"></param>
        /// <returns></returns>
        public static bool HasFlag(this ParserOptions en, ParserOptions cmp)
        {
            return (en & cmp) != 0;
        }
    }
#endif
}

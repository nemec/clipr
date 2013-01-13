using System;

namespace clipr
{
    /// <summary>
    /// The exception that is thrown when the set of arguments
    /// supplied to the parser are invalid.
    /// </summary>
    public class ArgumentIntegrityException : ArgumentException
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="ArgumentIntegrityException"/> class.
        /// </summary>
        /// <param name="message">Description of the exception.</param>
        internal ArgumentIntegrityException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="ArgumentIntegrityException"/> class.
        /// </summary>
        /// <param name="message">Description of the exception.</param>
        /// <param name="innerException">Exception being wrapped.</param>
        internal ArgumentIntegrityException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}

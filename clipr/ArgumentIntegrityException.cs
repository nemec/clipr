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
        public ArgumentIntegrityException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="ArgumentIntegrityException"/> class.
        /// </summary>
        /// <param name="message">Description of the exception.</param>
        /// <param name="innerException">Exception being wrapped.</param>
        public ArgumentIntegrityException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}

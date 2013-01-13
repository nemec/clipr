using System;

namespace clipr
{
    /// <summary>
    /// Represents an error that ocurred during parsing.
    /// </summary>
    public class ParseException : Exception
    {
        /// <summary>
        /// The parse token, if any, that caused the error.
        /// </summary>
        internal string Token { get; private set; }

        /// <summary>
        /// Create a new ParseException for a token.
        /// </summary>
        /// <param name="token"></param>
        internal ParseException(char token)
            : this(token.ToString())
        {
        }

        /// <summary>
        /// Create a new ParseException for a token and supply a message.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="message"></param>
        internal ParseException(char token, string message)
            : this(token.ToString(), message)
        {
        }

        /// <summary>
        /// Create a new ParseException for a token and supply a message
        /// and inner exception.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        internal ParseException(char token, string message, Exception innerException)
            : this(token.ToString(), message, innerException)
        {
        }

        /// <summary>
        /// Create a new ParseException for a token.
        /// </summary>
        /// <param name="token"></param>
        internal ParseException(string token)
        {
            Token = token;
        }

        /// <summary>
        /// Create a new ParseException for a token and supply a message.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="message"></param>
        internal ParseException(string token, string message)
            : base(message)
        {
            Token = token;
        }

        /// <summary>
        /// Create a new ParseException for a token and supply a message
        /// and inner exception.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="message"></param>
        /// <param name="innerException"></param>
        internal ParseException(string token, string message, Exception innerException)
            : base(message, innerException)
        {
            Token = token;
        }

        public override string ToString()
        {
            if (Token != null)
            {
                return String.Format(
                    @"At token ""{0}"": {1}", Token, base.ToString());
            }
            return base.ToString();
        }
    }
}

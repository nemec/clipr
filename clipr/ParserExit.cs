using System;

namespace clipr
{
    /// <summary>
    /// Thrown when the parser needs to immediately exit, such as
    /// after printing usage or version information.
    /// </summary>
    public class ParserExit : Exception
    {
        /// <summary>
        /// Error code to use when exiting.
        /// </summary>
        public int ExitCode { get; private set; }

        /// <summary>
        /// Create a new ParserExitException with an
        /// exit code of 0.
        /// </summary>
        public ParserExit()
        {
            ExitCode = 0;
        }

        /// <summary>
        /// Create a new ParserExitException with the
        /// given exit code.
        /// </summary>
        /// <param name="exitCode"></param>
        public ParserExit(int exitCode)
        {
            ExitCode = exitCode;
        }
    }
}

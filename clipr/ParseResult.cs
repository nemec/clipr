using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace clipr
{
    /// <summary>
    /// Extensions upon ParseResults
    /// </summary>
    public static class ParseResult
    {
        /// <summary>
        /// Built-in delegate provided to ParseResult.Handle to indicate
        /// that errors should be ignored.
        /// </summary>
        public static readonly Action<Exception[]> IgnoreErrors = e => { };
    }

    /// <summary>
    /// Contains the results of parsing a command, either a value or a list of errors.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ParseResult<T>
    {
        /// <summary>
        /// Indicates whether this ParseResult was initialized with a value or not.
        /// </summary>
        public readonly bool HasValue;

        private static readonly Exception[] EmptyErrors = new Exception[0];

        private readonly T Value;

        private readonly Exception[] Errors;

        /// <summary>
        /// Create a new ParseResult with the specified value.
        /// </summary>
        /// <param name="value"></param>
        public ParseResult(T value)
        {
            Value = value;
            HasValue = true;
            Errors = EmptyErrors;
        }

        /// <summary>
        /// Create a new error ParseResult with the specified errors.
        /// </summary>
        /// <param name="errors"></param>
        public ParseResult(params Exception[] errors)
        {
            Errors = errors;
        }

        /// <summary>
        /// Execute a delegate based on the contents of the result. If it contains
        /// a value, handleValue is called. Otherwise, handleErrors is called.
        /// </summary>
        /// <param name="handleValue"></param>
        /// <param name="handleErrors"></param>
        public void Handle(
            Action<T> handleValue,
            Action<Exception[]> handleErrors)
        {
            if (handleValue == null) throw new ArgumentException("handleValue cannot be null", "handleValue");
            if (handleErrors == null) throw new ArgumentException("handleErrors cannot be null", "handleVErrors");

            if (HasValue)
            {
                handleValue(Value);
            }
            else
            {
                handleErrors(Errors);
            }
        }

        /// <summary>
        /// Attempt to append another error to the result. If the original
        /// result contains a value, it fails as a result cannot contain
        /// both a value and errors.
        /// </summary>
        /// <param name="newResult">
        /// New result created after appending the new error. If the return
        /// value is false, newResult is the same instance as the original.
        /// </param>
        /// <param name="errors">New errors to append to the result.</param>
        /// <returns>true if an error was appended, false if the result contains a value.</returns>
        public bool TryAppendErrors(out ParseResult<T> newResult, params Exception[] errors)
        {
            if (HasValue)
            {
                newResult = this;
                return false;
            }

            var eh = Errors;
            var start = eh.Length;
            Array.Resize(ref eh, start + errors.Length);
            var end = errors.Length;
            for (var i = 0; i < end; ++i)
            {
                eh[start + i] = errors[i];
            }
            newResult = new ParseResult<T>(eh);
            return true;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            if (HasValue)
            {
                return String.Format("<ParseResult Value:{0}>", Value);
            }
            return String.Format("<ParseResult Errors({0}):{1}>",
                Errors.Length,
                String.Join(",", Errors
                    .Where(e => e != null)
                    .Select(e => e.GetType().Name)
                    .Distinct().ToArray()));
        }
    }
}

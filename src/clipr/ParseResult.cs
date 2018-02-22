using clipr.Triggers;
using System;
using System.Linq;

namespace clipr
{
    /// <summary>
    /// Contains the results of parsing a command, either a success, a trigger, or a list of errors.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ParseResult
    {
        public readonly bool IsSuccess;

        private readonly bool HasTrigger;

        private static readonly Exception[] EmptyErrors = new Exception[0];

        private readonly ITerminatingTrigger Trigger;

        private readonly Exception[] Errors;

        /// <summary>
        /// A successful result
        /// </summary>
        internal static readonly ParseResult Success = new ParseResult();

        private ParseResult()
        {
            IsSuccess = true;
            Errors = EmptyErrors;
        }

        /// <summary>
        /// Create a new ParseResult with the specified Trigger.
        /// </summary>
        /// <param name="trigger"></param>
        public ParseResult(ITerminatingTrigger trigger)
        {
            Trigger = trigger;
            HasTrigger = true;
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
        /// a value, handleValue is called. If it contains a TerminatingTrigger,
        /// handleTrigger is called. Otherwise, handleErrors is called.
        /// </summary>
        /// <param name="handleValue"></param>
        /// <param name="handleTrigger"></param>
        /// <param name="handleErrors"></param>
        public TRet Handle<TRet>(
            Func<TRet> handleSuccess,
            Func<ITerminatingTrigger, TRet> handleTrigger,
            Func<Exception[], TRet> handleErrors)
        {
            if (handleSuccess == null) throw new ArgumentException("handleSuccess cannot be null", "handleSuccess");
            if (handleTrigger == null) throw new ArgumentException("handleTrigger cannot be null", "handleTrigger");
            if (handleErrors == null) throw new ArgumentException("handleErrors cannot be null", "handleVErrors");

            if (IsSuccess) return handleSuccess();
            else if (HasTrigger) return handleTrigger(Trigger);
            else return handleErrors(Errors);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            if (IsSuccess)
            {
                return "<ParseResult Success>";
            }
            if (HasTrigger)
            {
                return String.Format("<ParseResult Trigger:{0}>", Trigger);
            }
            return String.Format("<ParseResult Errors({0}):{1}>",
                Errors.Length,
                String.Join(",", Errors
                    .Where(e => e != null)
                    .Select(e => e.GetType().Name)
                    .Distinct().ToArray()));
        }
    }

    /// <summary>
    /// Contains the results of parsing a command, either a value, a trigger, or a list of errors.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ParseResult<T>
    {
        private readonly bool HasValue;

        private readonly bool HasTrigger;

        private static readonly Exception[] EmptyErrors = new Exception[0];

        private readonly T Value;

        private readonly ITerminatingTrigger Trigger;

        private readonly Exception[] Errors;

        internal static readonly ParseResult<T> Empty = new ParseResult<T>(new Exception[0]);

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
        /// Create a new ParseResult with the specified Trigger.
        /// </summary>
        /// <param name="trigger"></param>
        public ParseResult(ITerminatingTrigger trigger)
        {
            Trigger = trigger;
            HasTrigger = true;
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
        /// a value, handleValue is called. If it contains a TerminatingTrigger,
        /// handleTrigger is called. Otherwise, handleErrors is called.
        /// </summary>
        /// <param name="handleValue"></param>
        /// <param name="handleTrigger"></param>
        /// <param name="handleErrors"></param>
        /// <returns>Transformed result after handling value.</returns>
        public TRet Handle<TRet>(
            Func<T, TRet> handleValue,
            Func<ITerminatingTrigger, TRet> handleTrigger,
            Func<Exception[], TRet> handleErrors)
        {
            if (handleValue == null) throw new ArgumentException("handleValue cannot be null", "handleValue");
            if (handleTrigger == null) throw new ArgumentException("handleTrigger cannot be null", "handleTrigger");
            if (handleErrors == null) throw new ArgumentException("handleErrors cannot be null", "handleVErrors");

            if (HasValue) return handleValue(Value);
            else if (HasTrigger) return handleTrigger(Trigger);
            else return handleErrors(Errors);
        }

        /// <summary>
        /// Execute a delegate based on the contents of the result. If it contains
        /// a value, handleValue is called. If it contains a TerminatingTrigger,
        /// handleTrigger is called. Otherwise, handleErrors is called.
        /// </summary>
        /// <param name="handleValue"></param>
        /// <param name="handleTrigger"></param>
        /// <param name="handleErrors"></param>
        public void Handle(
            Action<T> handleValue,
            Action<ITerminatingTrigger> handleTrigger,
            Action<Exception[]> handleErrors)
        {
            if (handleValue == null) throw new ArgumentException("handleValue cannot be null", "handleValue");
            if (handleTrigger == null) throw new ArgumentException("handleTrigger cannot be null", "handleTrigger");
            if (handleErrors == null) throw new ArgumentException("handleErrors cannot be null", "handleVErrors");

            if (HasValue) handleValue(Value);
            else if (HasTrigger) handleTrigger(Trigger);
            else handleErrors(Errors);
        }

        /// <summary>
        /// Get the parsed value or throw an exception.
        /// </summary>
        /// <exception cref="TriggerException">Thrown if parsing exited due to a trigger being tagged.</exception>
        /// <exception cref="AggregateException">
        /// Thrown if other errors (such as invalid input) occur during parsing. The aggregate
        /// contains the list errors found.
        /// </exception>
        /// <returns></returns>
        public T GetValueOrThrow()
        {
            if (HasValue) return Value;

            if (HasTrigger)
            {
                throw new TriggerException(Trigger);
            }

            throw new AggregateException(Errors);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            if (HasValue)
            {
                return String.Format("<ParseResult Value:{0}>", Value);
            }
            if (HasTrigger)
            {
                return String.Format("<ParseResult Trigger:{0}>", Trigger);
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

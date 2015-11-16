using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AggregateException = clipr.Utils.AggregateException;

namespace clipr.UnitTests
{
    public static class AssertEx
    {
        /// <summary>
        /// Run the action and ensure it throws the given exception.
        /// </summary>
        /// <typeparam name="TException"></typeparam>
        /// <param name="message">Case insensitive sub-string.</param>
        /// <param name="action"></param>
        public static void Throws<TException>(Action action, string message = null)
            where TException : Exception
        {
            try
            {
                action();
            }
            catch (TException e)
            {
                if (message != null &&
                    !e.Message.ToLowerInvariant().Contains(message.ToLowerInvariant()))
                {
                    Assert.Fail("Correct exception was caught, but did not contain the expected message.");
                }
                return;
            }
            catch (Exception e)
            {
                Assert.Fail("Action threw exception of unexpected type {0}\n{1}", 
                    e.GetType(), e);
            }
            Assert.Fail("Action did not throw an exception of type {0}.", 
                typeof(TException));
        }

        /// <summary>
        /// Run the action and ensure it throws the given exception. Additionally,
        /// can check that exception message contains the given case insensitive
        /// message.
        /// </summary>
        /// <typeparam name="TInnerException"></typeparam>
        /// <param name="action">Action that throws an exception.</param>
        /// <param name="message">Case insensitive sub-string.</param>
        public static void ThrowsAggregateContaining<TInnerException>(Action action, string message = null)
            where TInnerException : Exception
        {
            try
            {
                action();
            }
            catch (AggregateException e)
            {
                var exOfType = new List<TInnerException>();
                e.Handle(ex =>
                {
                    var tmp = ex as TInnerException;
                    if (tmp != null)
                    {
                        exOfType.Add(tmp);
                    }
                    return true;
                });

                if (!exOfType.Any())
                {
                    Assert.Fail("Aggregate exception was caught, but did " +
                                "not contain the expected exception type.\n{0}",
                                e.InnerException);
                }
                if (message != null && 
                    !exOfType.Any(eo =>
                        eo.Message.ToLowerInvariant().Contains(message.ToLowerInvariant())))
                {
                    Assert.Fail("Correct exception was caught, but inner exceptions did not contain the expected message.");
                }
                return;
            }
            catch (Exception e)
            {
                Assert.Fail("Action threw exception of unexpected type {0}\n{1}",
                    e.GetType(), e);
            }
            Assert.Fail("Action did not throw an exception of type {0}.", 
                typeof(TInnerException));
        }
    }
}

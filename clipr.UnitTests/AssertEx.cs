using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace clipr.UnitTests
{
    public static class AssertEx
    {
        public static void Throws<TException>(Action action)
            where TException : Exception
        {
            try
            {
                action();
            }
            catch (TException)
            {
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

        public static void ThrowsAggregateContaining<TInnerException>(Action action)
            where TInnerException : Exception
        {
            try
            {
                action();
            }
            catch (AggregateException e)
            {
                if (e.InnerException.GetType() != typeof (TInnerException))
                {
                    Assert.Fail("Aggregate exception was caught, but did " +
                                "not contain the expected exception type.\n{0}",
                                e.InnerException);
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

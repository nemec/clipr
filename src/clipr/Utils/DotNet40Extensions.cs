using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace clipr.Utils
{
#if NET40
    /// <summary>
    /// Extensions to convert .Net4.0-compatible code to the .Net4.5 syntax.
    /// </summary>
    public static class DotNet40Extensions
    {
        /// <summary>
        /// Retrieve the only custom attribute of the given type. If
        /// there is more than one, an exception will be thrown.
        /// </summary>
        /// <exception cref="AmbiguousMatchException">
        /// The member has multiple of the same custom attribute.
        /// </exception>
        /// <typeparam name="T">Attribute type to grab.</typeparam>
        /// <param name="member"></param>
        /// <returns></returns>
        public static T GetCustomAttribute<T>(this MemberInfo member) where T : Attribute
        {
            try
            {
                return (T) member.GetCustomAttributes(typeof (T), false).SingleOrDefault();
            }
            catch (InvalidOperationException e)
            {
                throw new AmbiguousMatchException(e.Message, e);
            }
        }

        /// <summary>
        /// Get all custom attributes of the given type, generically.
        /// </summary>
        /// <typeparam name="T">Attribute type to retrieve.</typeparam>
        /// <param name="member"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetCustomAttributes<T>(this MemberInfo member) where T : Attribute
        {
            return member.GetCustomAttributes(typeof(T), false).Cast<T>();
        }
    }
#endif
}

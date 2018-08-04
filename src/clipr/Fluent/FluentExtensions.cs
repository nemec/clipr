
using clipr.Fluent;

namespace clipr
{
    /// <summary>
    /// Poor man's template specialization.
    /// </summary>
    public static class FluentExtensions
    {
        /// <summary>
        /// Instead of storing a value, the integer value will 
        /// increment each time the argument is specified on the
        /// command line.
        /// </summary>
        /// <param name="ths"></param>
        /// <returns></returns>
        public static NamedArgumentBuilder<int> CountsInvocations(
            this NamedArgumentBuilder<int> ths)
        {
            ths.CountsInvocations();
            return ths;
        }

        /// <summary>
        /// Shortcut for storing a constant value of 'true'
        /// if this argument is specified.
        /// </summary>
        /// <param name="ths"></param>
        /// <returns></returns>
        public static NamedArgumentBuilder<bool> StoresTrue(
            this NamedArgumentBuilder<bool> ths)
        {
            ths.StoresTrue();
            return ths;
        }

        /// <summary>
        /// Shortcut for storing a constant value of 'true'
        /// if this argument is specified.
        /// </summary>
        /// <param name="ths"></param>
        /// <returns></returns>
        public static NamedArgumentBuilder<bool?> StoresTrue(
            this NamedArgumentBuilder<bool?> ths)
        {
            ths.StoresTrue();
            return ths;
        }

        /// <summary>
        /// Shortcut for storing a constant value of 'false'
        /// if this argument is specified.
        /// </summary>
        /// <param name="ths"></param>
        /// <returns></returns>
        public static NamedArgumentBuilder<bool> StoresFalse(
            this NamedArgumentBuilder<bool> ths)
        {
            ths.StoresFalse();
            return ths;
        }

        /// <summary>
        /// Shortcut for storing a constant value of 'false'
        /// if this argument is specified.
        /// </summary>
        /// <param name="ths"></param>
        /// <returns></returns>
        public static NamedArgumentBuilder<bool?> StoresFalse(
            this NamedArgumentBuilder<bool?> ths)
        {
            ths.StoresFalse();
            return ths;
        }
        /// <summary>
        /// Instead of storing a value, the integer value will 
        /// increment each time the argument is specified on the
        /// command line.
        /// </summary>
        /// <param name="ths"></param>
        /// <returns></returns>
        public static PositionalArgumentBuilder<int> CountsInvocations(
            this PositionalArgumentBuilder<int> ths)
        {
            ths.CountsInvocations();
            return ths;
        }

        /// <summary>
        /// Shortcut for storing a constant value of 'true'
        /// if this argument is specified.
        /// </summary>
        /// <param name="ths"></param>
        /// <returns></returns>
        public static PositionalArgumentBuilder<bool> StoresTrue(
            this PositionalArgumentBuilder<bool> ths)
        {
            ths.StoresTrue();
            return ths;
        }

        /// <summary>
        /// Shortcut for storing a constant value of 'true'
        /// if this argument is specified.
        /// </summary>
        /// <param name="ths"></param>
        /// <returns></returns>
        public static PositionalArgumentBuilder<bool?> StoresTrue(
            this PositionalArgumentBuilder<bool?> ths)
        {
            ths.StoresTrue();
            return ths;
        }

        /// <summary>
        /// Shortcut for storing a constant value of 'false'
        /// if this argument is specified.
        /// </summary>
        /// <param name="ths"></param>
        /// <returns></returns>
        public static PositionalArgumentBuilder<bool> StoresFalse(
            this PositionalArgumentBuilder<bool> ths)
        {
            ths.StoresFalse();
            return ths;
        }

        /// <summary>
        /// Shortcut for storing a constant value of 'false'
        /// if this argument is specified.
        /// </summary>
        /// <param name="ths"></param>
        /// <returns></returns>
        public static PositionalArgumentBuilder<bool?> StoresFalse(
            this PositionalArgumentBuilder<bool?> ths)
        {
            ths.StoresFalse();
            return ths;
        }
    }
}

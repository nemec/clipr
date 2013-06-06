
namespace clipr.Fluent
{
    /// <summary>
    /// Poor man's template specialization.
    /// </summary>
    public static class FluentExtensions
    {
        public static Named<TConfig, int> CountsInvocations<TConfig>(this Named<TConfig, int> named)
            where TConfig : class
        {
            named.CountsInvocations();
            return named;
        }

        public static Named<TConfig, bool> StoresTrue<TConfig>(this Named<TConfig, bool> named)
            where TConfig : class
        {
            named.StoresTrue();
            return named;
        }

        public static Named<TConfig, bool> StoresFalse<TConfig>(this Named<TConfig, bool> named)
            where TConfig : class
        {
            named.StoresFalse();
            return named;
        }
    }
}

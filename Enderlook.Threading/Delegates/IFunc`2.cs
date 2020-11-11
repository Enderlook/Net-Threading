namespace Enderlook.Threading
{
    /// <summary>
    /// Defines a delegate like type.
    /// </summary>
    /// <typeparam name="T1">Type of parameter.</typeparam>
    /// <typeparam name="T2">Type of return.</typeparam>
    public interface IFunc<T1, T2>
    {
        /// <summary>
        /// Executes the behaviour of this delegate like type.
        /// </summary>
        /// <param name="parameter">Paramameter of the function.</param>
        /// <returns>Result of the method.</returns>
        T2 Invoke(T1 parameter);
    }
}

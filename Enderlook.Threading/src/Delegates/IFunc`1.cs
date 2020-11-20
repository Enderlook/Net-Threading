namespace Enderlook.Threading
{
    /// <summary>
    /// Defines a delegate like type.
    /// </summary>
    /// <typeparam name="T1">Type of return.</typeparam>
    public interface IFunc<T1>
    {
        /// <summary>
        /// Executes the behaviour of this delegate like type.
        /// </summary>
        /// <returns>Result of the method.</returns>
        T1 Invoke();
    }
}

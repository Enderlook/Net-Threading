namespace Enderlook.Threading
{
    /// <summary>
    /// Defines a delegate like type.
    /// </summary>
    /// <typeparam name="T">Type of parameter.</typeparam>
    public interface IAction<T>
    {
        /// <summary>
        /// Executes the behaviour of this delegate like type.
        /// </summary>
        /// <param name="parameter">Paramameter of the function.</param>
        void Invoke(T parameter);
    }
}

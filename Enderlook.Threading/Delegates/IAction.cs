namespace Enderlook.Threading
{
    /// <summary>
    /// Defines a delegate like type.
    /// </summary>
    public interface IAction
    {
        /// <summary>
        /// Executes the behaviour of this delegate like type.
        /// </summary>
        void Invoke();
    }
}

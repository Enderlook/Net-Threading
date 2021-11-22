using System.Threading.Tasks;

namespace Enderlook.Threading
{
    /// <summary>
    /// Extension methods for <see cref="TaskFactory"/>.
    /// </summary>
    public static partial class TaskFactoryExtension
    {
        internal sealed class Tuple2<T1, T2>
        {
            public T1 Item1;
            public T2 Item2;
        }
    }
}

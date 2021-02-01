using System.Threading.Tasks;

namespace Enderlook.Threading
{
    /// <summary>
    /// Extension methods for <see cref="TaskFactory"/>.
    /// </summary>
    public static partial class TaskFactoryExtension
    {
        private const int PacksLength = 100;
        private static readonly object[] indexes;

        static TaskFactoryExtension()
        {
            indexes = new object[PacksLength];
            for (int i = 0; i < PacksLength; i++)
                indexes[i] = i;
        }
    }
}

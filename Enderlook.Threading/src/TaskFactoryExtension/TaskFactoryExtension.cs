using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static object GetBoxedInteger(int index)
        {
            Debug.Assert(index > 0 && index < PacksLength);
#if NET5_0_OR_GREATER
            return Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(indexes), index);
#else
            return indexes[index];
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ref T Get<T>(T[] array, object index)
        {
            int index_ = (int)index;
            Debug.Assert(index_ > 0 && index_ < array.Length);

#if NET5_0_OR_GREATER
            return ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(array), index_);
#else
            return ref array[index_];
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ref T Get<T>(T[] array, int index)
        {
            Debug.Assert(index > 0 && index < array.Length);

#if NET5_0_OR_GREATER
            return ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(array), index);
#else
            return ref array[index];
#endif
        }
    }
}

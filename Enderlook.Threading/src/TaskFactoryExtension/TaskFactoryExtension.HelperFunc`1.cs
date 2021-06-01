using System;
using System.Threading;
using System.Threading.Tasks;

namespace Enderlook.Threading
{
    public static partial class TaskFactoryExtension
    {
        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{TResult})"/>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TResult>(this TaskFactory source, Func<TResult> action)
            => source.StartNew(HelperFunc<TResult>.Basic, HelperFunc<TResult>.Create(action));

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{TResult}, CancellationToken)"/>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TResult>(this TaskFactory source, Func<TResult> action, CancellationToken cancellationToken)
            => source.StartNew(HelperFunc<TResult>.Basic, HelperFunc<TResult>.Create(action), cancellationToken);

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{TResult}, CancellationToken, TaskCreationOptions, TaskScheduler)"/>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TResult>(this TaskFactory source, Func<TResult> action, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
            => source.StartNew(HelperFunc<TResult>.Basic, HelperFunc<TResult>.Create(action), cancellationToken, creationOptions, scheduler);

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{TResult}, TaskCreationOptions)"/>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TResult>(this TaskFactory source, Func<TResult> action, TaskCreationOptions creationOptions)
            => source.StartNew(HelperFunc<TResult>.Basic, HelperFunc<TResult>.Create(action), creationOptions);

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{TResult})"/>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TResult>(this TaskFactory<TResult> source, Func<TResult> action)
            => source.StartNew(HelperFunc<TResult>.Basic, HelperFunc<TResult>.Create(action));

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{TResult}, CancellationToken)"/>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TResult>(this TaskFactory<TResult> source, Func<TResult> action, CancellationToken cancellationToken)
            => source.StartNew(HelperFunc<TResult>.Basic, HelperFunc<TResult>.Create(action), cancellationToken);

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{TResult}, CancellationToken, TaskCreationOptions, TaskScheduler)"/>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TResult>(this TaskFactory<TResult> source, Func<TResult> action, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
            => source.StartNew(HelperFunc<TResult>.Basic, HelperFunc<TResult>.Create(action), cancellationToken, creationOptions, scheduler);

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{TResult}, TaskCreationOptions)"/>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TResult>(this TaskFactory<TResult> source, Func<TResult> action, TaskCreationOptions creationOptions)
            => source.StartNew(HelperFunc<TResult>.Basic, HelperFunc<TResult>.Create(action), creationOptions);

        private class HelperFunc<TResult>
        {
            public static readonly Func<object, TResult> Basic = new HelperFunc<TResult>().BasicMethod; // Instance calls are more performant.

            private static readonly (Func<TResult> action, int isBeingUsed)[] packs = new (Func<TResult> action, int isBeingUsed)[PacksLength];
            private static int index;

            private TResult BasicMethod(object obj)
            {
                ref var pack = ref packs[(int)obj];
                var action = pack.action;
                pack.action = default;
                Interlocked.Exchange(ref pack.isBeingUsed, 0);
                return action();
            }

            public static object Create(Func<TResult> action)
            {
                int index_ = Interlocked.Increment(ref index) % PacksLength;

                ref var pack = ref packs[index_];
                while (Interlocked.Exchange(ref pack.isBeingUsed, 1) == 1) ;
                pack.action = action;

                return indexes[index_];
            }
        }
    }
}
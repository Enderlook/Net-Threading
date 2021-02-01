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
            public static readonly Func<object, TResult> Basic = BasicMethod;

            private static readonly HelperFunc<TResult>[] packs;
            private static int index;

            static HelperFunc()
            {
                packs = new HelperFunc<TResult>[PacksLength];
                for (int i = 0; i < PacksLength; i++)
                    packs[i] = new HelperFunc<TResult>();
            }

            private Func<TResult> action;
            private int isBeingUsed;

            private static TResult BasicMethod(object obj)
            {
                var pack = (HelperFunc<TResult>)obj;
                var action = pack.action;
                pack.action = null;
                Interlocked.Exchange(ref pack.isBeingUsed, 0);
                return action();
            }

            public static HelperFunc<TResult> Create(Func<TResult> action)
            {
                int index_ = Interlocked.Increment(ref index) % PacksLength;

                var pack = packs[index_];
                while (Interlocked.Exchange(ref pack.isBeingUsed, 1) == 1) ;
                pack.action = action;

                return pack;
            }
        }
    }
}
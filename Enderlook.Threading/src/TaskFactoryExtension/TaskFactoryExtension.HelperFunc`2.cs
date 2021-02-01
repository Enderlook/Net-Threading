using System;
using System.Threading;
using System.Threading.Tasks;

namespace Enderlook.Threading
{
    public static partial class TaskFactoryExtension
    {
        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{object, TResult}, object)"/>
        /// <typeparam name="TState">Type of state.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TState, TResult>(this TaskFactory source, Func<TState, TResult> action, TState state)
            => source.StartNew(HelperFunc<TState, TResult>.Basic, HelperFunc<TState, TResult>.Create(action, state));

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{object, TResult}, object, CancellationToken)"/>
        /// <typeparam name="TState">Type of state.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TState, TResult>(this TaskFactory source, Func<TState, TResult> action, TState state, CancellationToken cancellationToken)
            => source.StartNew(HelperFunc<TState, TResult>.Basic, HelperFunc<TState, TResult>.Create(action, state), cancellationToken);

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{object, TResult}, object, CancellationToken, TaskCreationOptions, TaskScheduler)"/>
        /// <typeparam name="TState">Type of state.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TState, TResult>(this TaskFactory source, Func<TState, TResult> action, TState state, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
            => source.StartNew(HelperFunc<TState, TResult>.Basic, HelperFunc<TState, TResult>.Create(action, state), cancellationToken, creationOptions, scheduler);

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{object, TResult}, object, TaskCreationOptions)"/>
        /// <typeparam name="TState">Type of state.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TState, TResult>(this TaskFactory source, Func<TState, TResult> action, TState state, TaskCreationOptions creationOptions)
            => source.StartNew(HelperFunc<TState, TResult>.Basic, HelperFunc<TState, TResult>.Create(action, state), creationOptions);

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{object, TResult}, object)"/>
        /// <typeparam name="TState">Type of state.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TState, TResult>(this TaskFactory<TResult> source, Func<TState, TResult> action, TState state)
            => source.StartNew(HelperFunc<TState, TResult>.Basic, HelperFunc<TState, TResult>.Create(action, state));

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{object, TResult}, object, CancellationToken)"/>
        /// <typeparam name="TState">Type of state.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TState, TResult>(this TaskFactory<TResult> source, Func<TState, TResult> action, TState state, CancellationToken cancellationToken)
            => source.StartNew(HelperFunc<TState, TResult>.Basic, HelperFunc<TState, TResult>.Create(action, state), cancellationToken);

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{object, TResult}, object, CancellationToken, TaskCreationOptions, TaskScheduler)"/>
        /// <typeparam name="TState">Type of state.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TState, TResult>(this TaskFactory<TResult> source, Func<TState, TResult> action, TState state, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
            => source.StartNew(HelperFunc<TState, TResult>.Basic, HelperFunc<TState, TResult>.Create(action, state), cancellationToken, creationOptions, scheduler);

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{object, TResult}, object, TaskCreationOptions)"/>
        /// <typeparam name="TState">Type of state.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TState, TResult>(this TaskFactory<TResult> source, Func<TState, TResult> action, TState state, TaskCreationOptions creationOptions)
            => source.StartNew(HelperFunc<TState, TResult>.Basic, HelperFunc<TState, TResult>.Create(action, state), creationOptions);

        private class HelperFunc<TState, TResult>
        {
            public static readonly Func<object, TResult> Basic = BasicMethod;

            private static readonly HelperFunc<TState, TResult>[] packs;
            private static int index;

            static HelperFunc()
            {
                packs = new HelperFunc<TState, TResult>[PacksLength];
                for (int i = 0; i < PacksLength; i++)
                    packs[i] = new HelperFunc<TState, TResult>();
            }

            private Func<TState, TResult> action;
            private TState state;
            private int isBeingUsed;

            private static TResult BasicMethod(object obj)
            {
                HelperFunc<TState, TResult> pack = (HelperFunc<TState, TResult>)obj;
                Func<TState, TResult> action = pack.action;
                TState state = pack.state;
                pack.action = null;
                pack.state = default;
                Interlocked.Exchange(ref pack.isBeingUsed, 0);
                return action(state);
            }

            public static HelperFunc<TState, TResult> Create(Func<TState, TResult> action, TState state)
            {
                int index_ = Interlocked.Increment(ref index) % PacksLength;

                HelperFunc<TState, TResult> pack = packs[index_];
                while (Interlocked.Exchange(ref pack.isBeingUsed, 1) == 1) ;
                pack.action = action;
                pack.state = state;

                return pack;
            }
        }
    }
}
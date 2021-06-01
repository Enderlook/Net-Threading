using System;
using System.Runtime.CompilerServices;
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
            public static readonly Func<object, TResult> Basic = new HelperFunc<TState, TResult>().BasicMethod; // Instance calls are more performant.

            private static readonly (Func<TState, TResult> action, TState state, int isBeingUsed)[] packs = new (Func<TState, TResult> action, TState state, int isBeingUsed)[PacksLength];
            private static int index;

            private TResult BasicMethod(object obj)
            {
                if (obj is Tuple<Func<TState, TResult>, TState> tuple)
                    return tuple.Item1(tuple.Item2);
                else
                {
                    ref var pack = ref Get(packs, obj);
                    var action = pack.action;
                    var state = pack.state;
                    pack.action = null;
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
                    if (RuntimeHelpers.IsReferenceOrContainsReferences<TState>())
#endif
                        pack.state = default;
                    Interlocked.Exchange(ref pack.isBeingUsed, 0);
                    return action(state);
                }
            }

            public static object Create(Func<TState, TResult> action, TState state)
            {
                var index_ = Interlocked.Increment(ref index) % PacksLength;

                var packs_ = packs;
                ref var pack = ref Get(packs_, index_);

                while (Interlocked.Exchange(ref pack.isBeingUsed, 1) == 1)
                {
                    if (index_ == 0)
                        return Tuple.Create(action, state);
                    pack = ref Get(packs_, --index_);
                }

                pack.action = action;
                pack.state = state;

                return GetBoxedInteger(index_);
            }
        }
    }
}
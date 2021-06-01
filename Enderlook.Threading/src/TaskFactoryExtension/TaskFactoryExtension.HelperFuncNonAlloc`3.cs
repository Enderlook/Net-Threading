using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Enderlook.Threading
{
    public static partial class TaskFactoryExtension
    {
        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{object, TResult}, object)"/>
        /// <typeparam name="TFunc">Type of function.</typeparam>
        /// <typeparam name="TState">Type of state.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TFunc, TState, TResult>(this TaskFactory source, TFunc function, TState state)
            where TFunc : IFunc<TState, TResult>
            => source.StartNew(HelperFuncNoAlloc<TFunc, TState, TResult>.Basic, HelperFuncNoAlloc<TFunc, TState, TResult>.Create(function, state));

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{object, TResult}, object, CancellationToken)"/>
        /// <typeparam name="TFunc">Type of function.</typeparam>
        /// <typeparam name="TState">Type of state.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TFunc, TState, TResult>(this TaskFactory source, TFunc function, TState state, CancellationToken cancellationToken)
            where TFunc : IFunc<TState, TResult>
            => source.StartNew(HelperFuncNoAlloc<TFunc, TState, TResult>.Basic, HelperFuncNoAlloc<TFunc, TState, TResult>.Create(function, state), cancellationToken);

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{object, TResult}, object, CancellationToken, TaskCreationOptions, TaskScheduler)"/>
        /// <typeparam name="TFunc">Type of function.</typeparam>
        /// <typeparam name="TState">Type of state.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TFunc, TState, TResult>(this TaskFactory source, TFunc function, TState state, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
            where TFunc : IFunc<TState, TResult>
            => source.StartNew(HelperFuncNoAlloc<TFunc, TState, TResult>.Basic, HelperFuncNoAlloc<TFunc, TState, TResult>.Create(function, state), cancellationToken, creationOptions, scheduler);

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{object, TResult}, object, TaskCreationOptions)"/>
        /// <typeparam name="TFunc">Type of function.</typeparam>
        /// <typeparam name="TState">Type of state.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TFunc, TState, TResult>(this TaskFactory source, TFunc function, TState state, TaskCreationOptions creationOptions)
            where TFunc : IFunc<TState, TResult>
            => source.StartNew(HelperFuncNoAlloc<TFunc, TState, TResult>.Basic, HelperFuncNoAlloc<TFunc, TState, TResult>.Create(function, state), creationOptions);


        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{object, TResult}, object)"/>
        /// <typeparam name="TFunc">Type of function.</typeparam>
        /// <typeparam name="TState">Type of state.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TFunc, TState, TResult>(this TaskFactory<TResult> source, TFunc function, TState state)
            where TFunc : IFunc<TState, TResult>
            => source.StartNew(HelperFuncNoAlloc<TFunc, TState, TResult>.Basic, HelperFuncNoAlloc<TFunc, TState, TResult>.Create(function, state));

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{object, TResult}, object, CancellationToken)"/>
        /// <typeparam name="TFunc">Type of function.</typeparam>
        /// <typeparam name="TState">Type of state.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TFunc, TState, TResult>(this TaskFactory<TResult> source, TFunc function, TState state, CancellationToken cancellationToken)
            where TFunc : IFunc<TState, TResult>
            => source.StartNew(HelperFuncNoAlloc<TFunc, TState, TResult>.Basic, HelperFuncNoAlloc<TFunc, TState, TResult>.Create(function, state), cancellationToken);

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{object, TResult}, object, CancellationToken, TaskCreationOptions, TaskScheduler)"/>
        /// <typeparam name="TFunc">Type of function.</typeparam>
        /// <typeparam name="TState">Type of state.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TFunc, TState, TResult>(this TaskFactory<TResult> source, TFunc function, TState state, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
            where TFunc : IFunc<TState, TResult>
            => source.StartNew(HelperFuncNoAlloc<TFunc, TState, TResult>.Basic, HelperFuncNoAlloc<TFunc, TState, TResult>.Create(function, state), cancellationToken, creationOptions, scheduler);

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{object, TResult}, object, TaskCreationOptions)"/>
        /// <typeparam name="TFunc">Type of function.</typeparam>
        /// <typeparam name="TState">Type of state.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TFunc, TState, TResult>(this TaskFactory<TResult> source, TFunc function, TState state, TaskCreationOptions creationOptions)
            where TFunc : IFunc<TState, TResult>
            => source.StartNew(HelperFuncNoAlloc<TFunc, TState, TResult>.Basic, HelperFuncNoAlloc<TFunc, TState, TResult>.Create(function, state), creationOptions);

        private class HelperFuncNoAlloc<TFunc, TState, TResult> where TFunc : IFunc<TState, TResult>
        {
            public static readonly Func<object, TResult> Basic = new HelperFuncNoAlloc<TFunc, TState, TResult>().BasicMethod; // Instance calls are more performant.

            private static readonly (TFunc action, TState state, int isBeingUsed)[] packs = new (TFunc action, TState state, int isBeingUsed)[PacksLength];
            private static int index;

            private TResult BasicMethod(object obj)
            {
                if (obj is Tuple<TFunc, TState> tuple)
                    return tuple.Item1.Invoke(tuple.Item2);
                else
                {
                    ref var pack = ref Get(packs, obj);
                    var action = pack.action;
                    var state = pack.state;
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
                    if (RuntimeHelpers.IsReferenceOrContainsReferences<TFunc>())
#endif
                        pack.action = default;
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
                    if (RuntimeHelpers.IsReferenceOrContainsReferences<TState>())
#endif
                        pack.state = default;
                    Interlocked.Exchange(ref pack.isBeingUsed, 0);
                    return action.Invoke(state);
                }
            }

            public static object Create(TFunc action, TState state)
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

                return indexes[index_];
            }
        }
    }
}

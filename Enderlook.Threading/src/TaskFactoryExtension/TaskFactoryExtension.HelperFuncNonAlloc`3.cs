using System;
using System.Collections.Concurrent;
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
                ref var pack = ref packs[(int)obj];
                var action = pack.action;
                var state = pack.state;
                pack.action = default;
                pack.state = default;
                Interlocked.Exchange(ref pack.isBeingUsed, 0);
                return action.Invoke(state);
            }

            public static object Create(TFunc action, TState state)
            {
                int index_ = Interlocked.Increment(ref index) % PacksLength;

                ref var pack = ref packs[index_];
                while (Interlocked.Exchange(ref pack.isBeingUsed, 1) == 1) ;
                pack.action = action;
                pack.state = state;

                return indexes[index_];
            }
        }
    }
}

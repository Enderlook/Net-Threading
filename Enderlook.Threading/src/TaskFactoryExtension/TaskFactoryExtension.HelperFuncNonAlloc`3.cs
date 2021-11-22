using Enderlook.Pools;

using System;
using System.Diagnostics;
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

            private TResult BasicMethod(object obj)
            {
                Debug.Assert(obj is Tuple2<TFunc, TState>);
                Tuple2<TFunc, TState> tuple = Unsafe.As<Tuple2<TFunc, TState>>(obj);
                TFunc function = tuple.Item1;
                TState state = tuple.Item2;
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
                if (RuntimeHelpers.IsReferenceOrContainsReferences<TFunc>())
#endif
                    tuple.Item1 = default;
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
                if (RuntimeHelpers.IsReferenceOrContainsReferences<TState>())
#endif
                    tuple.Item2 = default;
                ObjectPool<Tuple2<TFunc, TState>>.Shared.Return(tuple);
                return function.Invoke(state);
            }

            public static object Create(TFunc function, TState state)
            {
                Tuple2<TFunc, TState> tuple = ObjectPool<Tuple2<TFunc, TState>>.Shared.Rent();
                tuple.Item1 = function;
                tuple.Item2 = state;
                return tuple;
            }
        }
    }
}

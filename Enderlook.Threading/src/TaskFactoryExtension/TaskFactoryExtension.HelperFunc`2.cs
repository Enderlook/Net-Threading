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
        /// <typeparam name="TState">Type of state.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TState, TResult>(this TaskFactory source, Func<TState, TResult> function, TState state)
            => source.StartNew(HelperFunc<TState, TResult>.Basic, HelperFunc<TState, TResult>.Create(function, state));

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{object, TResult}, object, CancellationToken)"/>
        /// <typeparam name="TState">Type of state.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TState, TResult>(this TaskFactory source, Func<TState, TResult> function, TState state, CancellationToken cancellationToken)
            => source.StartNew(HelperFunc<TState, TResult>.Basic, HelperFunc<TState, TResult>.Create(function, state), cancellationToken);

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{object, TResult}, object, CancellationToken, TaskCreationOptions, TaskScheduler)"/>
        /// <typeparam name="TState">Type of state.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TState, TResult>(this TaskFactory source, Func<TState, TResult> function, TState state, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
            => source.StartNew(HelperFunc<TState, TResult>.Basic, HelperFunc<TState, TResult>.Create(function, state), cancellationToken, creationOptions, scheduler);

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{object, TResult}, object, TaskCreationOptions)"/>
        /// <typeparam name="TState">Type of state.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TState, TResult>(this TaskFactory source, Func<TState, TResult> function, TState state, TaskCreationOptions creationOptions)
            => source.StartNew(HelperFunc<TState, TResult>.Basic, HelperFunc<TState, TResult>.Create(function, state), creationOptions);

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{object, TResult}, object)"/>
        /// <typeparam name="TState">Type of state.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TState, TResult>(this TaskFactory<TResult> source, Func<TState, TResult> function, TState state)
            => source.StartNew(HelperFunc<TState, TResult>.Basic, HelperFunc<TState, TResult>.Create(function, state));

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{object, TResult}, object, CancellationToken)"/>
        /// <typeparam name="TState">Type of state.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TState, TResult>(this TaskFactory<TResult> source, Func<TState, TResult> function, TState state, CancellationToken cancellationToken)
            => source.StartNew(HelperFunc<TState, TResult>.Basic, HelperFunc<TState, TResult>.Create(function, state), cancellationToken);

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{object, TResult}, object, CancellationToken, TaskCreationOptions, TaskScheduler)"/>
        /// <typeparam name="TState">Type of state.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TState, TResult>(this TaskFactory<TResult> source, Func<TState, TResult> function, TState state, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
            => source.StartNew(HelperFunc<TState, TResult>.Basic, HelperFunc<TState, TResult>.Create(function, state), cancellationToken, creationOptions, scheduler);

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{object, TResult}, object, TaskCreationOptions)"/>
        /// <typeparam name="TState">Type of state.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TState, TResult>(this TaskFactory<TResult> source, Func<TState, TResult> function, TState state, TaskCreationOptions creationOptions)
            => source.StartNew(HelperFunc<TState, TResult>.Basic, HelperFunc<TState, TResult>.Create(function, state), creationOptions);

        private class HelperFunc<TState, TResult>
        {
            public static readonly Func<object, TResult> Basic = new HelperFunc<TState, TResult>().BasicMethod; // Instance calls are more performant.

            private TResult BasicMethod(object obj)
            {
                Debug.Assert(obj is Tuple2<Func<TState, TResult>, TState>);
                Tuple2<Func<TState, TResult>, TState> tuple = Unsafe.As<Tuple2<Func<TState, TResult>, TState>>(obj);
                Func<TState, TResult> function = tuple.Item1;
                TState state = tuple.Item2;
                tuple.Item1 = null;
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
                if (RuntimeHelpers.IsReferenceOrContainsReferences<TState>())
#endif
                    tuple.Item2 = default;
                ObjectPool<Tuple2<Func<TState, TResult>, TState>>.Shared.Return(tuple);
                return function(state);
            }

            public static object Create(Func<TState, TResult> function, TState state)
            {
                Tuple2<Func<TState, TResult>, TState> tuple = ObjectPool<Tuple2<Func<TState, TResult>, TState>>.Shared.Rent();
                tuple.Item1 = function;
                tuple.Item2 = state;
                return tuple;
            }
        }
    }
}
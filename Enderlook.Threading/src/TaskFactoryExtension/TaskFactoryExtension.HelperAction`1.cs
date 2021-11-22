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
        /// <inheritdoc cref="TaskFactory.StartNew(Action{object}, object)"/>
        /// <typeparam name="TState">Type of state.</typeparam>
        public static Task StartNew<TState>(this TaskFactory source, Action<TState> action, TState state)
            => source.StartNew(HelperAction<TState>.Basic, HelperAction<TState>.Create(action, state));

        /// <inheritdoc cref="TaskFactory.StartNew(Action{object}, object, CancellationToken)"/>
        /// <typeparam name="TState">Type of state.</typeparam>
        public static Task StartNew<TState>(this TaskFactory source, Action<TState> action, TState state, CancellationToken cancellationToken)
            => source.StartNew(HelperAction<TState>.Basic, HelperAction<TState>.Create(action, state), cancellationToken);

        /// <inheritdoc cref="TaskFactory.StartNew(Action{object}, object, CancellationToken, TaskCreationOptions, TaskScheduler)"/>
        /// <typeparam name="TState">Type of state.</typeparam>
        public static Task StartNew<TState>(this TaskFactory source, Action<TState> action, TState state, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
            => source.StartNew(HelperAction<TState>.Basic, HelperAction<TState>.Create(action, state), cancellationToken, creationOptions, scheduler);

        /// <inheritdoc cref="TaskFactory.StartNew(Action{object}, object, TaskCreationOptions)"/>
        /// <typeparam name="TState">Type of state.</typeparam>
        public static Task StartNew<TState>(this TaskFactory source, Action<TState> action, TState state, TaskCreationOptions creationOptions)
            => source.StartNew(HelperAction<TState>.Basic, HelperAction<TState>.Create(action, state), creationOptions);

        private class HelperAction<TState>
        {
            public static readonly Action<object> Basic = new HelperAction<TState>().BasicMethod; // Instance calls are more performant.

            private void BasicMethod(object obj)
            {
                Debug.Assert(obj is Tuple2<Action<TState>, TState>);
                Tuple2<Action<TState>, TState> tuple = Unsafe.As<Tuple2<Action<TState>, TState>>(obj);
                Action<TState> action = tuple.Item1;
                TState state = tuple.Item2;
                tuple.Item1 = null;
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
                if (RuntimeHelpers.IsReferenceOrContainsReferences<TState>())
#endif
                    tuple.Item2 = default;
                ObjectPool<Tuple2<Action<TState>, TState>>.Shared.Return(tuple);
                action(state);
            }

            public static object Create(Action<TState> action, TState state)
            {
                Tuple2<Action<TState>, TState> tuple = ObjectPool<Tuple2<Action<TState>, TState>>.Shared.Rent();
                tuple.Item1 = action;
                tuple.Item2 = state;
                return tuple;
            }
        }
    }
}

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
        /// <typeparam name="TAction">Type of action.</typeparam>
        /// <typeparam name="TState">Type of state.</typeparam>
        public static Task StartNew<TAction, TState>(this TaskFactory source, TAction action, TState state)
            where TAction : IAction<TState>
            => source.StartNew(HelperActionNonAlloc<TAction, TState>.Basic, HelperActionNonAlloc<TAction, TState>.Create(action, state));

        /// <inheritdoc cref="TaskFactory.StartNew(Action{object}, object, CancellationToken)"/>
        /// <typeparam name="TAction">Type of action.</typeparam>
        /// <typeparam name="TState">Type of state.</typeparam>
        public static Task StartNew<TAction, TState>(this TaskFactory source, TAction action, TState state, CancellationToken cancellationToken)
            where TAction : IAction<TState>
            => source.StartNew(HelperActionNonAlloc<TAction, TState>.Basic, HelperActionNonAlloc<TAction, TState>.Create(action, state), cancellationToken);

        /// <inheritdoc cref="TaskFactory.StartNew(Action{object}, object, CancellationToken, TaskCreationOptions, TaskScheduler)"/>
        /// <typeparam name="TAction">Type of action.</typeparam>
        /// <typeparam name="TState">Type of state.</typeparam>
        public static Task StartNew<TAction, TState>(this TaskFactory source, TAction action, TState state, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
            where TAction : IAction<TState>
            => source.StartNew(HelperActionNonAlloc<TAction, TState>.Basic, HelperActionNonAlloc<TAction, TState>.Create(action, state), cancellationToken, creationOptions, scheduler);

        /// <inheritdoc cref="TaskFactory.StartNew(Action{object}, object, TaskCreationOptions)"/>
        /// <typeparam name="TAction">Type of action.</typeparam>
        /// <typeparam name="TState">Type of state.</typeparam>
        public static Task StartNew<TAction, TState>(this TaskFactory source, TAction action, TState state, TaskCreationOptions creationOptions)
            where TAction : IAction<TState>
            => source.StartNew(HelperActionNonAlloc<TAction, TState>.Basic, HelperActionNonAlloc<TAction, TState>.Create(action, state), creationOptions);

        private class HelperActionNonAlloc<TAction, TState> where TAction : IAction<TState>
        {
            public static readonly Action<object> Basic = new HelperActionNonAlloc<TAction, TState>().BasicMethod; // Instance calls are more performant.

            private void BasicMethod(object obj)
            {
                Debug.Assert(obj is Tuple2<TAction, TState>);
                Tuple2<TAction, TState> tuple = Unsafe.As<Tuple2<TAction, TState>>(obj);
                TAction action = tuple.Item1;
                TState state = tuple.Item2;
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
                if (RuntimeHelpers.IsReferenceOrContainsReferences<TAction>())
#endif
                    tuple.Item1 = default;
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
                if (RuntimeHelpers.IsReferenceOrContainsReferences<TState>())
#endif
                    tuple.Item2 = default;
                ObjectPool<Tuple2<TAction, TState>>.Shared.Return(tuple);
                action.Invoke(state);
            }

            public static object Create(TAction action, TState state)
            {
                Tuple2<TAction, TState> tuple = ObjectPool<Tuple2<TAction, TState>>.Shared.Rent();
                tuple.Item1 = action;
                tuple.Item2 = state;
                return tuple;
            }
        }
    }
}
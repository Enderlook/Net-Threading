using System;
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

            private static readonly (TAction action, TState state, int isBeingUsed)[] packs = new (TAction action, TState state, int isBeingUsed)[PacksLength];
            private static int index;

            private void BasicMethod(object obj)
            {
                ref var pack = ref packs[(int)obj];
                var action = pack.action;
                var state = pack.state;
                pack.action = default;
                pack.state = default;
                Interlocked.Exchange(ref pack.isBeingUsed, 0);
                action.Invoke(state);
            }

            public static object Create(TAction action, TState state)
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
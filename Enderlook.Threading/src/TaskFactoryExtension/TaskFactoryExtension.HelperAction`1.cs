using System;
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

        private static class HelperAction<TState>
        {
            public static readonly Action<object> Basic = BasicMethod;

            private static readonly (Action<TState> action, TState state, int isBeingUsed)[] packs = new (Action<TState> action, TState state, int isBeingUsed)[PacksLength];
            private static int index;

            private static void BasicMethod(object obj)
            {
                ref var pack = ref packs[(int)obj];
                var action = pack.action;
                var state = pack.state;
                pack.action = null;
                pack.state = default;
                Interlocked.Exchange(ref pack.isBeingUsed, 0);
                action(state);
            }

            public static object Create(Action<TState> action, TState state)
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

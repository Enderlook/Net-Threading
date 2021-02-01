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

        private class HelperAction<TState>
        {
            public static readonly Action<object> Basic = BasicMethod;

            private static readonly HelperAction<TState>[] packs;
            private static int index;

            static HelperAction()
            {
                packs = new HelperAction<TState>[PacksLength];
                for (int i = 0; i < PacksLength; i++)
                    packs[i] = new HelperAction<TState>();
            }

            private TState state;
            private Action<TState> action;
            private int isBeingUsed;

            private static void BasicMethod(object obj)
            {
                var pack = (HelperAction<TState>)obj;
                var action = pack.action;
                var state = pack.state;
                pack.action = null;
                pack.state = default;
                Interlocked.Exchange(ref pack.isBeingUsed, 0);
                action(state);
            }

            public static HelperAction<TState> Create(Action<TState> action, TState state)
            {
                var index_ = Interlocked.Increment(ref index) % PacksLength;

                var pack = packs[index_];
                while (Interlocked.Exchange(ref pack.isBeingUsed, 1) == 1) ;
                pack.action = action;
                pack.state = state;

                return pack;
            }
        }
    }
}

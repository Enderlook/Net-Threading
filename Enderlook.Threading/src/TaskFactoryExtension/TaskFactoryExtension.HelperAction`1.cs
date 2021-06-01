using System;
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

            private static readonly (Action<TState> action, TState state, int isBeingUsed)[] packs = new (Action<TState> action, TState state, int isBeingUsed)[PacksLength];
            private static int index;

            private void BasicMethod(object obj)
            {
                if (obj is Tuple<Action<TState>, TState> tuple)
                    tuple.Item1(tuple.Item2);
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
                    action(state);
                }
            }

            public static object Create(Action<TState> action, TState state)
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

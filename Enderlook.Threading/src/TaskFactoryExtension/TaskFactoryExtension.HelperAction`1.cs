using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Enderlook.Threading
{
    public static partial class TaskFactoryExtension
    {
        /// <inheritdoc cref="TaskFactory.StartNew(Action{object}, object)"/>
        /// <typeparam name="TState">Type of state.</typeparam>
        public static Task StartNew<TState>(this TaskFactory source, Action<TState> action, TState state)
            => source.StartNew(HelperAction<TState>.Basic, HelperAction<TState>.Pack.Create(action, state));

        /// <inheritdoc cref="TaskFactory.StartNew(Action{object}, object, CancellationToken)"/>
        /// <typeparam name="TState">Type of state.</typeparam>
        public static Task StartNew<TState>(this TaskFactory source, Action<TState> action, TState state, CancellationToken cancellationToken)
            => source.StartNew(HelperAction<TState>.Basic, HelperAction<TState>.Pack.Create(action, state), cancellationToken);

        /// <inheritdoc cref="TaskFactory.StartNew(Action{object}, object, CancellationToken, TaskCreationOptions, TaskScheduler)"/>
        /// <typeparam name="TState">Type of state.</typeparam>
        public static Task StartNew<TState>(this TaskFactory source, Action<TState> action, TState state, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
            => source.StartNew(HelperAction<TState>.Basic, HelperAction<TState>.Pack.Create(action, state), cancellationToken, creationOptions, scheduler);

        /// <inheritdoc cref="TaskFactory.StartNew(Action{object}, object, TaskCreationOptions)"/>
        /// <typeparam name="TState">Type of state.</typeparam>
        public static Task StartNew<TState>(this TaskFactory source, Action<TState> action, TState state, TaskCreationOptions creationOptions)
            => source.StartNew(HelperAction<TState>.Basic, HelperAction<TState>.Pack.Create(action, state), creationOptions);

        private static class HelperAction<TState>
        {
            public static readonly Action<object> Basic = BasicMethod;

            private static void BasicMethod(object obj) => ((Pack)obj).Run();

            public sealed class Pack
            {
                private static ConcurrentBag<Pack> packs = new ConcurrentBag<Pack>();

                private Action<TState> action;
                private TState state;

                private Pack(Action<TState> action, TState state)
                {
                    this.action = action;
                    this.state = state;
                }

                public void Run()
                {
                    action(state);
                    action = null;
                    state = default;
                    packs.Add(this);
                }

                public static Pack Create(Action<TState> action, TState state)
                {
                    if (packs.TryTake(out Pack pack))
                    {
                        pack.Set(action, state);
                        return pack;
                    }
                    return new Pack(action, state);
                }

                private void Set(Action<TState> action, TState state)
                {
                    this.action = action;
                    this.state = state;
                }
            }
        }
    }
}

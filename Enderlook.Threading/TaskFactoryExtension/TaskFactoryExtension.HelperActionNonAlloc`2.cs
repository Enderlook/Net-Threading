using System;
using System.Collections.Concurrent;
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
            => source.StartNew(HelperActionNoAlloc<TAction, TState>.Basic, HelperActionNoAlloc<TAction, TState>.Pack.Create(action, state));

        /// <inheritdoc cref="TaskFactory.StartNew(Action{object}, object, CancellationToken)"/>
        /// <typeparam name="TAction">Type of action.</typeparam>
        /// <typeparam name="TState">Type of state.</typeparam>
        public static Task StartNew<TAction, TState>(this TaskFactory source, TAction action, TState state, CancellationToken cancellationToken)
            where TAction : IAction<TState>
            => source.StartNew(HelperActionNoAlloc<TAction, TState>.Basic, HelperActionNoAlloc<TAction, TState>.Pack.Create(action, state), cancellationToken);

        /// <inheritdoc cref="TaskFactory.StartNew(Action{object}, object, CancellationToken, TaskCreationOptions, TaskScheduler)"/>
        /// <typeparam name="TAction">Type of action.</typeparam>
        /// <typeparam name="TState">Type of state.</typeparam>
        public static Task StartNew<TAction, TState>(this TaskFactory source, TAction action, TState state, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
            where TAction : IAction<TState>
            => source.StartNew(HelperActionNoAlloc<TAction, TState>.Basic, HelperActionNoAlloc<TAction, TState>.Pack.Create(action, state), cancellationToken, creationOptions, scheduler);

        /// <inheritdoc cref="TaskFactory.StartNew(Action{object}, object, TaskCreationOptions)"/>
        /// <typeparam name="TAction">Type of action.</typeparam>
        /// <typeparam name="TState">Type of state.</typeparam>
        public static Task StartNew<TAction, TState>(this TaskFactory source, TAction action, TState state, TaskCreationOptions creationOptions)
            where TAction : IAction<TState>
            => source.StartNew(HelperActionNoAlloc<TAction, TState>.Basic, HelperActionNoAlloc<TAction, TState>.Pack.Create(action, state), creationOptions);

        private static class HelperActionNoAlloc<TAction, TState> where TAction : IAction<TState>
        {
            public static readonly Action<object> Basic = BasicMethod;

            private static void BasicMethod(object obj) => ((Pack)obj).Run();

            public sealed class Pack
            {
                private static ConcurrentBag<Pack> packs = new ConcurrentBag<Pack>();

                private TAction action;
                private TState state;

                private Pack(TAction action, TState state)
                {
                    this.action = action;
                    this.state = state;
                }

                public void Run()
                {
                    action.Invoke(state);
                    action = default;
                    packs.Add(this);
                }

                public static Pack Create(TAction action, TState state)
                {
                    if (packs.TryTake(out Pack pack))
                    {
                        pack.Set(action, state);
                        return pack;
                    }
                    return new Pack(action, state);
                }

                private void Set(TAction action, TState state)
                {
                    this.action = action;
                    this.state = state;
                }
            }
        }
    }
}
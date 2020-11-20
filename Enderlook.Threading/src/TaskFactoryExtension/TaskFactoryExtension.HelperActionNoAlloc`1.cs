using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Enderlook.Threading
{
    public static partial class TaskFactoryExtension
    {
        /// <inheritdoc cref="TaskFactory.StartNew(Action)"/>
        /// <typeparam name="TAction">Type of action.</typeparam>
        public static Task StartNew<TAction>(this TaskFactory source, TAction action)
            where TAction : IAction
            => source.StartNew(HelperActionNoAlloc<TAction>.Basic, HelperActionNoAlloc<TAction>.Pack.Create(action));

        /// <inheritdoc cref="TaskFactory.StartNew(Action, CancellationToken)"/>
        /// <typeparam name="TAction">Type of action.</typeparam>
        public static Task StartNew<TAction>(this TaskFactory source, TAction action, CancellationToken cancellationToken)
            where TAction : IAction
            => source.StartNew(HelperActionNoAlloc<TAction>.Basic, HelperActionNoAlloc<TAction>.Pack.Create(action), cancellationToken);

        /// <inheritdoc cref="TaskFactory.StartNew(Action, CancellationToken, TaskCreationOptions, TaskScheduler)"/>
        /// <typeparam name="TAction">Type of action.</typeparam>
        public static Task StartNew<TAction>(this TaskFactory source, TAction action, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
            where TAction : IAction
            => source.StartNew(HelperActionNoAlloc<TAction>.Basic, HelperActionNoAlloc<TAction>.Pack.Create(action), cancellationToken, creationOptions, scheduler);

        /// <inheritdoc cref="TaskFactory.StartNew(Action, TaskCreationOptions)"/>
        /// <typeparam name="TAction">Type of action.</typeparam>
        public static Task StartNew<TAction>(this TaskFactory source, TAction action, TaskCreationOptions creationOptions)
            where TAction : IAction
            => source.StartNew(HelperActionNoAlloc<TAction>.Basic, HelperActionNoAlloc<TAction>.Pack.Create(action), creationOptions);

        private static class HelperActionNoAlloc<TAction> where TAction : IAction
        {
            public static readonly Action<object> Basic = BasicMethod;

            private static void BasicMethod(object obj) => ((Pack)obj).Run();

            public sealed class Pack
            {
                private static ConcurrentBag<Pack> packs = new ConcurrentBag<Pack>();

                private TAction action;

                private Pack(TAction action) => this.action = action;

                public void Run()
                {
                    action.Invoke();
                    action = default;
                    packs.Add(this);
                }

                public static Pack Create(TAction action)
                {
                    if (packs.TryTake(out Pack pack))
                    {
                        pack.Set(action);
                        return pack;
                    }
                    return new Pack(action);
                }

                private void Set(TAction action) => this.action = action;
            }
        }
    }
}

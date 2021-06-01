using System;
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
            => source.StartNew(HelperActionNonAlloc<TAction>.Basic, HelperActionNonAlloc<TAction>.Create(action));

        /// <inheritdoc cref="TaskFactory.StartNew(Action, CancellationToken)"/>
        /// <typeparam name="TAction">Type of action.</typeparam>
        public static Task StartNew<TAction>(this TaskFactory source, TAction action, CancellationToken cancellationToken)
            where TAction : IAction
            => source.StartNew(HelperActionNonAlloc<TAction>.Basic, HelperActionNonAlloc<TAction>.Create(action), cancellationToken);

        /// <inheritdoc cref="TaskFactory.StartNew(Action, CancellationToken, TaskCreationOptions, TaskScheduler)"/>
        /// <typeparam name="TAction">Type of action.</typeparam>
        public static Task StartNew<TAction>(this TaskFactory source, TAction action, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
            where TAction : IAction
            => source.StartNew(HelperActionNonAlloc<TAction>.Basic, HelperActionNonAlloc<TAction>.Create(action), cancellationToken, creationOptions, scheduler);

        /// <inheritdoc cref="TaskFactory.StartNew(Action, TaskCreationOptions)"/>
        /// <typeparam name="TAction">Type of action.</typeparam>
        public static Task StartNew<TAction>(this TaskFactory source, TAction action, TaskCreationOptions creationOptions)
            where TAction : IAction
            => source.StartNew(HelperActionNonAlloc<TAction>.Basic, HelperActionNonAlloc<TAction>.Create(action), creationOptions);

        private class HelperActionNonAlloc<TAction> where TAction : IAction
        {
            public static readonly Action<object> Basic = new HelperActionNonAlloc<TAction>().BasicMethod; // Instance calls are more performant..

            private static readonly (TAction action, int isBeingUsed)[] packs = new (TAction action, int isBeingUsed)[PacksLength];
            private static int index;

            private void BasicMethod(object obj)
            {
                ref var pack = ref packs[(int)obj];
                var action = pack.action;
                pack.action = default;
                Interlocked.Exchange(ref pack.isBeingUsed, 0);
                action.Invoke();
            }

            public static object Create(TAction action)
            {
                int index_ = Interlocked.Increment(ref index) % PacksLength;

                ref var pack = ref packs[index_];
                while (Interlocked.Exchange(ref pack.isBeingUsed, 1) == 1) ;
                pack.action = action;

                return indexes[index_];
            }
        }
    }
}

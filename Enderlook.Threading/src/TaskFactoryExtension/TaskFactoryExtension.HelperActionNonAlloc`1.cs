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
            public static readonly Action<object> Basic = new HelperActionNonAlloc<TAction>().BasicMethod; // Instance calls are more performant.

            private void BasicMethod(object obj)
            {
                Debug.Assert(obj is StrongBox<TAction>);
                StrongBox<TAction> box = Unsafe.As<StrongBox<TAction>>(obj);
                TAction action = box.Value;
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
                if (RuntimeHelpers.IsReferenceOrContainsReferences<TAction>())
#endif
                    box.Value = default;
                ObjectPool<StrongBox<TAction>>.Shared.Return(box);
                action.Invoke();
            }

            public static object Create(TAction action)
            {
                StrongBox<TAction> box = ObjectPool<StrongBox<TAction>>.Shared.Rent();
                box.Value = action;
                return box;
            }
        }
    }
}

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Enderlook.Threading
{
    public static partial class TaskFactoryExtension
    {
        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{object, TResult}, object)"/>
        /// <typeparam name="TFunc">Type of function.</typeparam>
        /// <typeparam name="TState">Type of state.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TFunc, TState, TResult>(this TaskFactory source, TFunc function, TState state)
            where TFunc : IFunc<TState, TResult>
            => source.StartNew(HelperFuncNoAlloc<TFunc, TState, TResult>.Basic, HelperFuncNoAlloc<TFunc, TState, TResult>.Create(function, state));

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{object, TResult}, object, CancellationToken)"/>
        /// <typeparam name="TFunc">Type of function.</typeparam>
        /// <typeparam name="TState">Type of state.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TFunc, TState, TResult>(this TaskFactory source, TFunc function, TState state, CancellationToken cancellationToken)
            where TFunc : IFunc<TState, TResult>
            => source.StartNew(HelperFuncNoAlloc<TFunc, TState, TResult>.Basic, HelperFuncNoAlloc<TFunc, TState, TResult>.Create(function, state), cancellationToken);

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{object, TResult}, object, CancellationToken, TaskCreationOptions, TaskScheduler)"/>
        /// <typeparam name="TFunc">Type of function.</typeparam>
        /// <typeparam name="TState">Type of state.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TFunc, TState, TResult>(this TaskFactory source, TFunc function, TState state, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
            where TFunc : IFunc<TState, TResult>
            => source.StartNew(HelperFuncNoAlloc<TFunc, TState, TResult>.Basic, HelperFuncNoAlloc<TFunc, TState, TResult>.Create(function, state), cancellationToken, creationOptions, scheduler);

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{object, TResult}, object, TaskCreationOptions)"/>
        /// <typeparam name="TFunc">Type of function.</typeparam>
        /// <typeparam name="TState">Type of state.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TFunc, TState, TResult>(this TaskFactory source, TFunc function, TState state, TaskCreationOptions creationOptions)
            where TFunc : IFunc<TState, TResult>
            => source.StartNew(HelperFuncNoAlloc<TFunc, TState, TResult>.Basic, HelperFuncNoAlloc<TFunc, TState, TResult>.Create(function, state), creationOptions);


        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{object, TResult}, object)"/>
        /// <typeparam name="TFunc">Type of function.</typeparam>
        /// <typeparam name="TState">Type of state.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TFunc, TState, TResult>(this TaskFactory<TResult> source, TFunc function, TState state)
            where TFunc : IFunc<TState, TResult>
            => source.StartNew(HelperFuncNoAlloc<TFunc, TState, TResult>.Basic, HelperFuncNoAlloc<TFunc, TState, TResult>.Create(function, state));

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{object, TResult}, object, CancellationToken)"/>
        /// <typeparam name="TFunc">Type of function.</typeparam>
        /// <typeparam name="TState">Type of state.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TFunc, TState, TResult>(this TaskFactory<TResult> source, TFunc function, TState state, CancellationToken cancellationToken)
            where TFunc : IFunc<TState, TResult>
            => source.StartNew(HelperFuncNoAlloc<TFunc, TState, TResult>.Basic, HelperFuncNoAlloc<TFunc, TState, TResult>.Create(function, state), cancellationToken);

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{object, TResult}, object, CancellationToken, TaskCreationOptions, TaskScheduler)"/>
        /// <typeparam name="TFunc">Type of function.</typeparam>
        /// <typeparam name="TState">Type of state.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TFunc, TState, TResult>(this TaskFactory<TResult> source, TFunc function, TState state, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
            where TFunc : IFunc<TState, TResult>
            => source.StartNew(HelperFuncNoAlloc<TFunc, TState, TResult>.Basic, HelperFuncNoAlloc<TFunc, TState, TResult>.Create(function, state), cancellationToken, creationOptions, scheduler);

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{object, TResult}, object, TaskCreationOptions)"/>
        /// <typeparam name="TFunc">Type of function.</typeparam>
        /// <typeparam name="TState">Type of state.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TFunc, TState, TResult>(this TaskFactory<TResult> source, TFunc function, TState state, TaskCreationOptions creationOptions)
            where TFunc : IFunc<TState, TResult>
            => source.StartNew(HelperFuncNoAlloc<TFunc, TState, TResult>.Basic, HelperFuncNoAlloc<TFunc, TState, TResult>.Create(function, state), creationOptions);

        private class HelperFuncNoAlloc<TFunc, TState, TResult> where TFunc : IFunc<TState, TResult>
        {
            public static readonly Func<object, TResult> Basic = BasicMethod;

            private static readonly HelperFuncNoAlloc<TFunc, TState, TResult>[] packs;
            private static int index;

            static HelperFuncNoAlloc()
            {
                packs = new HelperFuncNoAlloc<TFunc, TState, TResult>[PacksLength];
                for (int i = 0; i < PacksLength; i++)
                    packs[i] = new HelperFuncNoAlloc<TFunc, TState, TResult>();
            }

            private TFunc action;
            private TState state;
            private int isBeingUsed;

            private static TResult BasicMethod(object obj)
            {
                HelperFuncNoAlloc<TFunc, TState, TResult> pack = (HelperFuncNoAlloc<TFunc, TState, TResult>)obj;
                TFunc action = pack.action;
                TState state = pack.state;
                pack.action = default;
                pack.state = default;
                Interlocked.Exchange(ref pack.isBeingUsed, 0);
                return action.Invoke(state);
            }

            public static HelperFuncNoAlloc<TFunc, TState, TResult> Create(TFunc action, TState state)
            {
                int index_ = Interlocked.Increment(ref index) % PacksLength;

                HelperFuncNoAlloc<TFunc, TState, TResult> pack = packs[index_];
                while (Interlocked.Exchange(ref pack.isBeingUsed, 1) == 1) ;
                pack.action = action;
                pack.state = state;

                return pack;
            }
        }
    }
}

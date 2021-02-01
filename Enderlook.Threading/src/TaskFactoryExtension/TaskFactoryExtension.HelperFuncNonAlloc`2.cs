using System;
using System.Threading;
using System.Threading.Tasks;

namespace Enderlook.Threading
{
    public static partial class TaskFactoryExtension
    {
        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{TResult})"/>
        /// <typeparam name="TFunc">Type of function.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TFunc, TResult>(this TaskFactory source, TFunc function)
            where TFunc : IFunc<TResult>
            => source.StartNew(HelperFuncNoAlloc<TFunc, TResult>.Basic, HelperFuncNoAlloc<TFunc, TResult>.Create(function));

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{TResult}, CancellationToken)"/>
        /// <typeparam name="TFunc">Type of function.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TFunc, TResult>(this TaskFactory source, TFunc function, CancellationToken cancellationToken)
            where TFunc : IFunc<TResult>
            => source.StartNew(HelperFuncNoAlloc<TFunc, TResult>.Basic, HelperFuncNoAlloc<TFunc, TResult>.Create(function), cancellationToken);

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{TResult}, CancellationToken, TaskCreationOptions, TaskScheduler)"/>
        /// <typeparam name="TFunc">Type of function.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TFunc, TResult>(this TaskFactory source, TFunc function, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
            where TFunc : IFunc<TResult>
            => source.StartNew(HelperFuncNoAlloc<TFunc, TResult>.Basic, HelperFuncNoAlloc<TFunc, TResult>.Create(function), cancellationToken, creationOptions, scheduler);

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{TResult}, TaskCreationOptions)"/>
        /// <typeparam name="TFunc">Type of function.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TFunc, TResult>(this TaskFactory source, TFunc function, TaskCreationOptions creationOptions)
            where TFunc : IFunc<TResult>
            => source.StartNew(HelperFuncNoAlloc<TFunc, TResult>.Basic, HelperFuncNoAlloc<TFunc, TResult>.Create(function), creationOptions);

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{TResult})"/>
        /// <typeparam name="TFunc">Type of function.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TFunc, TResult>(this TaskFactory<TResult> source, TFunc function)
            where TFunc : IFunc<TResult>
            => source.StartNew(HelperFuncNoAlloc<TFunc, TResult>.Basic, HelperFuncNoAlloc<TFunc, TResult>.Create(function));

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{TResult}, CancellationToken)"/>
        /// <typeparam name="TFunc">Type of function.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TFunc, TResult>(this TaskFactory<TResult> source, TFunc function, CancellationToken cancellationToken)
            where TFunc : IFunc<TResult>
            => source.StartNew(HelperFuncNoAlloc<TFunc, TResult>.Basic, HelperFuncNoAlloc<TFunc, TResult>.Create(function), cancellationToken);

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{TResult}, CancellationToken, TaskCreationOptions, TaskScheduler)"/>
        /// <typeparam name="TFunc">Type of function.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TFunc, TResult>(this TaskFactory<TResult> source, TFunc function, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
            where TFunc : IFunc<TResult>
            => source.StartNew(HelperFuncNoAlloc<TFunc, TResult>.Basic, HelperFuncNoAlloc<TFunc, TResult>.Create(function), cancellationToken, creationOptions, scheduler);

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{TResult}, TaskCreationOptions)"/>
        /// <typeparam name="TFunc">Type of function.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TFunc, TResult>(this TaskFactory<TResult> source, TFunc function, TaskCreationOptions creationOptions)
            where TFunc : IFunc<TResult>
            => source.StartNew(HelperFuncNoAlloc<TFunc, TResult>.Basic, HelperFuncNoAlloc<TFunc, TResult>.Create(function), creationOptions);

        private class HelperFuncNoAlloc<TFunc, TResult> where TFunc : IFunc<TResult>
        {
            public static readonly Func<object, TResult> Basic = BasicMethod;

            private static readonly HelperFuncNoAlloc<TFunc, TResult>[] packs;
            private static int index;

            static HelperFuncNoAlloc()
            {
                packs = new HelperFuncNoAlloc<TFunc, TResult>[PacksLength];
                for (int i = 0; i < PacksLength; i++)
                    packs[i] = new HelperFuncNoAlloc<TFunc, TResult>();
            }

            private TFunc action;
            private int isBeingUsed;

            private static TResult BasicMethod(object obj)
            {
                HelperFuncNoAlloc<TFunc, TResult> pack = (HelperFuncNoAlloc<TFunc, TResult>)obj;
                TFunc action = pack.action;
                pack.action = default;
                Interlocked.Exchange(ref pack.isBeingUsed, 0);
                return action.Invoke();
            }

            public static HelperFuncNoAlloc<TFunc, TResult> Create(TFunc action)
            {
                int index_ = Interlocked.Increment(ref index) % PacksLength;

                HelperFuncNoAlloc<TFunc, TResult> pack = packs[index_];
                while (Interlocked.Exchange(ref pack.isBeingUsed, 1) == 1) ;
                pack.action = action;

                return pack;
            }
        }
    }
}
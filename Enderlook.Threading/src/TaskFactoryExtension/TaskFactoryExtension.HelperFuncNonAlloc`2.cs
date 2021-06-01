﻿using System;
using System.Runtime.CompilerServices;
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
            public static readonly Func<object, TResult> Basic = new HelperFuncNoAlloc<TFunc, TResult>().BasicMethod; // Instance calls are more performant.

            private static readonly (TFunc action, int isBeingUsed)[] packs = new (TFunc action, int isBeingUsed)[PacksLength];
            private static int index;

            private TResult BasicMethod(object obj)
            {
                if (obj is Tuple<TFunc> tuple)
                    return tuple.Item1.Invoke();
                else
                {
                    ref var pack = ref Get(packs, obj);
                    var action = pack.action;
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
                    if (RuntimeHelpers.IsReferenceOrContainsReferences<TFunc>())
#endif
                        pack.action = default;
                    Interlocked.Exchange(ref pack.isBeingUsed, 0);
                    return action.Invoke();
                }
            }

            public static object Create(TFunc action)
            {
                var index_ = Interlocked.Increment(ref index) % PacksLength;

                var packs_ = packs;
                ref var pack = ref Get(packs_, index_);

                while (Interlocked.Exchange(ref pack.isBeingUsed, 1) == 1)
                {
                    if (index_ == 0)
                        return Tuple.Create(action);
                    pack = ref Get(packs_, --index_);
                }

                pack.action = action;

                return indexes[index_];
            }
        }
    }
}
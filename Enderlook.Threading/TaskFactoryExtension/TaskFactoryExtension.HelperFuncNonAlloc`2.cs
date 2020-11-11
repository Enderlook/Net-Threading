using System;
using System.Collections.Concurrent;
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
            => source.StartNew(HelperFuncNoAlloc<TFunc, TResult>.Basic, HelperFuncNoAlloc<TFunc, TResult>.Pack.Create(function));

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{TResult}, CancellationToken)"/>
        /// <typeparam name="TFunc">Type of function.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TFunc, TResult>(this TaskFactory source, TFunc function, CancellationToken cancellationToken)
            where TFunc : IFunc<TResult>
            => source.StartNew(HelperFuncNoAlloc<TFunc, TResult>.Basic, HelperFuncNoAlloc<TFunc, TResult>.Pack.Create(function), cancellationToken);

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{TResult}, CancellationToken, TaskCreationOptions, TaskScheduler)"/>
        /// <typeparam name="TFunc">Type of function.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TFunc, TResult>(this TaskFactory source, TFunc function, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
            where TFunc : IFunc<TResult>
            => source.StartNew(HelperFuncNoAlloc<TFunc, TResult>.Basic, HelperFuncNoAlloc<TFunc, TResult>.Pack.Create(function), cancellationToken, creationOptions, scheduler);

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{TResult}, TaskCreationOptions)"/>
        /// <typeparam name="TFunc">Type of function.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TFunc, TResult>(this TaskFactory source, TFunc function, TaskCreationOptions creationOptions)
            where TFunc : IFunc<TResult>
            => source.StartNew(HelperFuncNoAlloc<TFunc, TResult>.Basic, HelperFuncNoAlloc<TFunc, TResult>.Pack.Create(function), creationOptions);

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{TResult})"/>
        /// <typeparam name="TFunc">Type of function.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TFunc, TResult>(this TaskFactory<TResult> source, TFunc function)
            where TFunc : IFunc<TResult>
            => source.StartNew(HelperFuncNoAlloc<TFunc, TResult>.Basic, HelperFuncNoAlloc<TFunc, TResult>.Pack.Create(function));

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{TResult}, CancellationToken)"/>
        /// <typeparam name="TFunc">Type of function.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TFunc, TResult>(this TaskFactory<TResult> source, TFunc function, CancellationToken cancellationToken)
            where TFunc : IFunc<TResult>
            => source.StartNew(HelperFuncNoAlloc<TFunc, TResult>.Basic, HelperFuncNoAlloc<TFunc, TResult>.Pack.Create(function), cancellationToken);

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{TResult}, CancellationToken, TaskCreationOptions, TaskScheduler)"/>
        /// <typeparam name="TFunc">Type of function.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TFunc, TResult>(this TaskFactory<TResult> source, TFunc function, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
            where TFunc : IFunc<TResult>
            => source.StartNew(HelperFuncNoAlloc<TFunc, TResult>.Basic, HelperFuncNoAlloc<TFunc, TResult>.Pack.Create(function), cancellationToken, creationOptions, scheduler);

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{TResult}, TaskCreationOptions)"/>
        /// <typeparam name="TFunc">Type of function.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TFunc, TResult>(this TaskFactory<TResult> source, TFunc function, TaskCreationOptions creationOptions)
            where TFunc : IFunc<TResult>
            => source.StartNew(HelperFuncNoAlloc<TFunc, TResult>.Basic, HelperFuncNoAlloc<TFunc, TResult>.Pack.Create(function), creationOptions);

        private static class HelperFuncNoAlloc<TFunc, TResult> where TFunc : IFunc<TResult>
        {
            public static readonly Func<object, TResult> Basic = BasicMethod;

            private static TResult BasicMethod(object obj) => ((Pack)obj).Run();

            public sealed class Pack
            {
                private static ConcurrentBag<Pack> packs = new ConcurrentBag<Pack>();

                private TFunc function;

                private Pack(TFunc function) => this.function = function;

                public TResult Run()
                {
                    TResult result = function.Invoke();
                    function = default;
                    packs.Add(this);
                    return result;
                }

                public static Pack Create(TFunc function)
                {
                    if (packs.TryTake(out Pack pack))
                    {
                        pack.Set(function);
                        return pack;
                    }
                    return new Pack(function);
                }

                private void Set(TFunc function) => this.function = function;
            }
        }
    }
}
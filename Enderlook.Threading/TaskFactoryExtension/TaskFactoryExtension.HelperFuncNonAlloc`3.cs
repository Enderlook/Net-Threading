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
            => source.StartNew(HelperFuncNoAlloc<TFunc, TState, TResult>.Basic, HelperFuncNoAlloc<TFunc, TState, TResult>.Pack.Create(function, state));

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{object, TResult}, object, CancellationToken)"/>
        /// <typeparam name="TFunc">Type of function.</typeparam>
        /// <typeparam name="TState">Type of state.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TFunc, TState, TResult>(this TaskFactory source, TFunc function, TState state, CancellationToken cancellationToken)
            where TFunc : IFunc<TState, TResult>
            => source.StartNew(HelperFuncNoAlloc<TFunc, TState, TResult>.Basic, HelperFuncNoAlloc<TFunc, TState, TResult>.Pack.Create(function, state), cancellationToken);

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{object, TResult}, object, CancellationToken, TaskCreationOptions, TaskScheduler)"/>
        /// <typeparam name="TFunc">Type of function.</typeparam>
        /// <typeparam name="TState">Type of state.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TFunc, TState, TResult>(this TaskFactory source, TFunc function, TState state, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
            where TFunc : IFunc<TState, TResult>
            => source.StartNew(HelperFuncNoAlloc<TFunc, TState, TResult>.Basic, HelperFuncNoAlloc<TFunc, TState, TResult>.Pack.Create(function, state), cancellationToken, creationOptions, scheduler);

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{object, TResult}, object, TaskCreationOptions)"/>
        /// <typeparam name="TFunc">Type of function.</typeparam>
        /// <typeparam name="TState">Type of state.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TFunc, TState, TResult>(this TaskFactory source, TFunc function, TState state, TaskCreationOptions creationOptions)
            where TFunc : IFunc<TState, TResult>
            => source.StartNew(HelperFuncNoAlloc<TFunc, TState, TResult>.Basic, HelperFuncNoAlloc<TFunc, TState, TResult>.Pack.Create(function, state), creationOptions);


        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{object, TResult}, object)"/>
        /// <typeparam name="TFunc">Type of function.</typeparam>
        /// <typeparam name="TState">Type of state.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TFunc, TState, TResult>(this TaskFactory<TResult> source, TFunc function, TState state)
            where TFunc : IFunc<TState, TResult>
            => source.StartNew(HelperFuncNoAlloc<TFunc, TState, TResult>.Basic, HelperFuncNoAlloc<TFunc, TState, TResult>.Pack.Create(function, state));

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{object, TResult}, object, CancellationToken)"/>
        /// <typeparam name="TFunc">Type of function.</typeparam>
        /// <typeparam name="TState">Type of state.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TFunc, TState, TResult>(this TaskFactory<TResult> source, TFunc function, TState state, CancellationToken cancellationToken)
            where TFunc : IFunc<TState, TResult>
            => source.StartNew(HelperFuncNoAlloc<TFunc, TState, TResult>.Basic, HelperFuncNoAlloc<TFunc, TState, TResult>.Pack.Create(function, state), cancellationToken);

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{object, TResult}, object, CancellationToken, TaskCreationOptions, TaskScheduler)"/>
        /// <typeparam name="TFunc">Type of function.</typeparam>
        /// <typeparam name="TState">Type of state.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TFunc, TState, TResult>(this TaskFactory<TResult> source, TFunc function, TState state, CancellationToken cancellationToken, TaskCreationOptions creationOptions, TaskScheduler scheduler)
            where TFunc : IFunc<TState, TResult>
            => source.StartNew(HelperFuncNoAlloc<TFunc, TState, TResult>.Basic, HelperFuncNoAlloc<TFunc, TState, TResult>.Pack.Create(function, state), cancellationToken, creationOptions, scheduler);

        /// <inheritdoc cref="TaskFactory.StartNew{TResult}(Func{object, TResult}, object, TaskCreationOptions)"/>
        /// <typeparam name="TFunc">Type of function.</typeparam>
        /// <typeparam name="TState">Type of state.</typeparam>
        /// <typeparam name="TResult">Type of result.</typeparam>
        public static Task<TResult> StartNew<TFunc, TState, TResult>(this TaskFactory<TResult> source, TFunc function, TState state, TaskCreationOptions creationOptions)
            where TFunc : IFunc<TState, TResult>
            => source.StartNew(HelperFuncNoAlloc<TFunc, TState, TResult>.Basic, HelperFuncNoAlloc<TFunc, TState, TResult>.Pack.Create(function, state), creationOptions);

        private static class HelperFuncNoAlloc<TFunc, TState, TResult> where TFunc : IFunc<TState, TResult>
        {
            public static readonly Func<object, TResult> Basic = BasicMethod;

            private static TResult BasicMethod(object obj) => ((Pack)obj).Run();

            public sealed class Pack
            {
                private static ConcurrentBag<Pack> packs = new ConcurrentBag<Pack>();

                private TFunc function;
                private TState state;

                private Pack(TFunc function, TState state)
                {
                    this.function = function;
                    this.state = state;
                }

                public TResult Run()
                {
                    TResult result = function.Invoke(state);
                    function = default;
                    packs.Add(this);
                    return result;
                }

                public static Pack Create(TFunc function, TState state)
                {
                    if (packs.TryTake(out Pack pack))
                    {
                        pack.Set(function, state);
                        return pack;
                    }
                    return new Pack(function, state);
                }

                private void Set(TFunc function, TState state)
                {
                    this.function = function;
                    this.state = state;
                }
            }
        }
        }
}
